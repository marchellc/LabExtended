using LabExtended.API;
using LabExtended.API.CustomCommands.Formatting;

using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parsing;
using LabExtended.Commands.CustomData;

using UnityEngine;

using NorthwoodLib.Pools;

using System.Reflection;

namespace LabExtended.Commands
{
    public static class CommandParser
    {
        private static readonly Dictionary<Type, ICommandParser> _definedParsers = new()
        {
            [typeof(string)] = new StringParser(),
            [typeof(bool)] = new BoolParser(),

            [typeof(sbyte)] = new NumericParser(typeof(sbyte)),
            [typeof(byte)] = new NumericParser(typeof(byte)),

            [typeof(ushort)] = new NumericParser(typeof(ushort)),
            [typeof(short)] = new NumericParser(typeof(short)),

            [typeof(uint)] = new NumericParser(typeof(uint)),
            [typeof(int)] = new NumericParser(typeof(int)),

            [typeof(ulong)] = new NumericParser(typeof(ulong)),
            [typeof(long)] = new NumericParser(typeof(long)),

            [typeof(float)] = new NumericParser(typeof(float)),
            [typeof(decimal)] = new NumericParser(typeof(decimal)),
            [typeof(double)] = new NumericParser(typeof(double)),

            [typeof(TimeSpan)] = new TimeSpanParser(),
            [typeof(DateTime)] = new DateTimeParser(),

            [typeof(Color)] = new ColorParser(),

            [typeof(Vector2)] = new Vector2Parser(),
            [typeof(Vector3)] = new Vector3Parser(),

            [typeof(Quaternion)] = new QuaternionParser(),

            [typeof(ExPlayer)] = new PlayerParser(),
            [typeof(PlayerListData)] = new PlayerListParser()
        };

        private static readonly Dictionary<char, char> _definedQuotes = new()
        {
            ['\''] = '\''
        };

        static CommandParser()
        {
            var types = typeof(ServerConsole).Assembly.GetTypes().ToList();

            types.AddRange(typeof(CommandParser).Assembly.GetTypes());

            foreach (var type in types)
            {
                if (!type.IsEnum || FormattingEnumCommand.EnumTypes.Any(t => t.Name == type.Name))
                    continue;

                FormattingEnumCommand.EnumTypes.Add(type);
            }
        }

        public static IReadOnlyDictionary<Type, ICommandParser> Parsers => _definedParsers;
        public static IReadOnlyDictionary<char, char> Quotes => _definedQuotes;

        public static void SetParser(Type type, ICommandParser parser)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (parser is null)
                throw new ArgumentNullException(nameof(parser));

            _definedParsers[type] = parser;
        }

        public static ICommandParser FromType<T>()
            => FromType(typeof(T));

        public static ICommandParser FromType(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var underlyingNullable = Nullable.GetUnderlyingType(type);

            if (underlyingNullable != null)
                type = underlyingNullable;

            if (_definedParsers.TryGetValue(type, out var parser))
                return parser;

            if (type.IsEnum)
                return _definedParsers[type] = new EnumParser(type);

            if (type.GetTypeInfo().IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();

                if (definition == typeof(List<>))
                {
                    var elementType = type.GetGenericArguments()[0];

                    if (elementType.IsEnum)
                        return new ListParser(new EnumParser(elementType), elementType);

                    if (elementType is null || !TryGetParser(elementType, out var elementParser))
                        throw new Exception($"Missing parser for list hintElement type: {elementType?.FullName ?? "null"} ({type.FullName})");

                    return _definedParsers[type] = new ListParser(elementParser, elementType);
                }

                if (definition == typeof(Dictionary<,>))
                {
                    var genericArgs = type.GetGenericArguments();

                    var keyParser = default(ICommandParser);
                    var valueParser = default(ICommandParser);

                    if (genericArgs[0].IsEnum)
                        keyParser = new EnumParser(genericArgs[0]);
                    else if (!TryGetParser(genericArgs[0], out keyParser))
                        throw new Exception($"Missing parser for dictionary key type: {genericArgs[0].FullName} ({type.FullName})");

                    if (genericArgs[1].IsEnum)
                        valueParser = new EnumParser(genericArgs[1]);
                    else if (!TryGetParser(genericArgs[1], out valueParser))
                        throw new Exception($"Missing parser for dictionary value type: {genericArgs[1].FullName} ({type.FullName})");

                    return _definedParsers[type] = new DictionaryParser(keyParser, valueParser, genericArgs[0], genericArgs[1]);
                }
            }

            throw new Exception($"Missing parser for argument type: {type.FullName}");
        }

        public static bool TryGetParser(Type type, out ICommandParser parser)
            => (parser = FromType(type)) != null;

        public static bool TryParseDefaultArgs(ExPlayer sender, string arg, ArgumentDefinition[] args, ArgumentCollection collection, out ArgumentDefinition failedArg, out string failedReason)
            => InternalParseArgs(sender, arg, args, (parsedArg, parsedValue) => collection.Add(parsedArg.Name, parsedValue), () => collection.Size, out failedArg, out failedReason);

        public static bool TryParseCustomArgs(ExPlayer sender, string arg, ArgumentDefinition[] args, object[] parsedArgs, out ArgumentDefinition failedArg, out string failedReason)
        {
            var list = ListPool<object>.Shared.Rent();
            var success = InternalParseArgs(sender, arg, args, (parsedArg, parsedValue) => list.Add(parsedValue), () => list.Count, out failedArg, out failedReason);

            for (int i = 0; i < list.Count; i++)
                parsedArgs[i + 1] = list[i];

            ListPool<object>.Shared.Return(list);
            return success;
        }

        internal enum StringPart
        {
            None,
            Parameter,
            QuotedParameter
        }

        internal static bool IsOpen(char character)
            => Quotes.ContainsKey(character) || character == '\"';

        internal static char GetMatch(char character)
            => Quotes.TryGetValue(character, out var match) ? match : '\"';

        internal static bool InternalParseArgs(ExPlayer sender, string arg, ArgumentDefinition[] args, Action<ArgumentDefinition, object> setArgumentValue, Func<int> countArgs, out ArgumentDefinition failedArg, out string failedReason)
        {
            failedArg = null;
            failedReason = null;

            if (args.Length == 1 && args[0].Type == typeof(string))
            {
                setArgumentValue(args[0], arg.Trim());
                return true;
            }

            var curArg = default(ArgumentDefinition);
            var curBuilder = StringBuilderPool.Shared.Rent();
            var curPart = StringPart.None;

            var endIndex = arg.Length;
            var prevIndex = int.MinValue;

            var isEscaping = false;

            var curChar = '\0';
            var curMatch = '\0';

            for (int i = 0; i <= endIndex; i++)
            {
                if (i < endIndex)
                    curChar = arg[i];
                else
                    curChar = '\0';

                if (curArg != null && ((countArgs() + 1) >= args.Length) && i != endIndex)
                {
                    curBuilder.Append(curChar);
                    continue;
                }

                if (isEscaping)
                {
                    if (i != endIndex)
                    {
                        if (curChar != curMatch)
                            curBuilder.Append('\\');

                        curBuilder.Append(curChar);
                        isEscaping = false;

                        continue;
                    }
                }

                if (curChar == '\\' && (curArg is null || !((countArgs() + 1) >= args.Length)))
                {
                    isEscaping = true;
                    continue;
                }

                if (curPart is StringPart.None)
                {
                    if (char.IsWhiteSpace(curChar) || i == endIndex)
                    {
                        continue;
                    }
                    else if (i == prevIndex)
                    {
                        StringBuilderPool.Shared.Return(curBuilder);

                        failedArg = curArg;
                        failedReason = "There must be at least one character of whitespace between arguments.";

                        return false;
                    }
                    else
                    {
                        if (curArg is null)
                            curArg = args.Length > countArgs() ? args[countArgs()] : null;

                        if (curArg != null && ((countArgs() + 1) >= args.Length))
                        {
                            curBuilder.Append(curChar);
                            continue;
                        }

                        if (IsOpen(curChar))
                        {
                            curPart = StringPart.QuotedParameter;
                            curMatch = GetMatch(curChar);

                            continue;
                        }

                        curPart = StringPart.Parameter;
                    }
                }

                var argString = default(string);

                if (curPart is StringPart.Parameter)
                {
                    if (i == endIndex || char.IsWhiteSpace(curChar))
                    {
                        argString = curBuilder.ToString();
                        prevIndex = i;
                    }
                    else
                    {
                        curBuilder.Append(curChar);
                    }
                }
                else if (curPart is StringPart.QuotedParameter)
                {
                    if (curChar == curMatch)
                    {
                        argString = curBuilder.ToString();
                        prevIndex = i + 1;
                    }
                    else
                    {
                        curBuilder.Append(curChar);
                    }
                }

                if (argString != null)
                {
                    if (curArg is null)
                    {
                        break;
                    }
                    
                    if (!CommandPropertyParser.TryParse(sender, curArg.Parser, argString, out var result)
                        && !curArg.Parser.TryParse(sender, argString, out var failureMessage, out result))
                    {
                        StringBuilderPool.Shared.Return(curBuilder);

                        failedReason = failureMessage;
                        failedArg = curArg;

                        return false;
                    }

                    setArgumentValue(curArg, result);

                    curPart = StringPart.None;
                    curBuilder.Clear();
                    curArg = null;
                }
            }

            if (curArg != null && ((countArgs() + 1) >= args.Length))
            {
                if (!CommandPropertyParser.TryParse(sender, curArg.Parser, curBuilder.ToString(), out var remainderResult)
                    && !curArg.Parser.TryParse(sender, curBuilder.ToString(), out var failureMessage, out remainderResult))
                {
                    StringBuilderPool.Shared.Return(curBuilder);

                    failedReason = failureMessage;
                    failedArg = curArg;

                    return false;
                }

                setArgumentValue(curArg, remainderResult);
                curArg = null;
            }

            if (isEscaping)
            {
                StringBuilderPool.Shared.Return(curBuilder);

                failedArg = curArg;
                failedReason = "Input text may not end on an incomplete escape.";

                return false;
            }

            if (curPart is StringPart.QuotedParameter)
            {
                StringBuilderPool.Shared.Return(curBuilder);

                failedArg = curArg;
                failedReason = "A quoted parameter is incomplete.";

                return false;
            }

            for (int i = countArgs(); i < args.Length; i++)
            {
                curArg = args[i];

                if (!curArg.IsOptional)
                {
                    StringBuilderPool.Shared.Return(curBuilder);

                    failedArg = curArg;
                    failedReason = "The input text has too few parameters.";

                    return false;
                }

                setArgumentValue(curArg, curArg.Default);
            }

            StringBuilderPool.Shared.Return(curBuilder);
            return true;
        }
    }
}