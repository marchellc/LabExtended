using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Parsing;
using LabExtended.Core.Profiling;

using LabExtended.Commands.Formatting;

using LabExtended.API;

using Common.Pooling.Pools;

using UnityEngine;

namespace LabExtended.Core.Commands
{
    public static class CommandUtils
    {
        private static readonly ProfilerMarker _marker = new ProfilerMarker("Commands (Parsing)");

        static CommandUtils()
        {
            var types = typeof(ServerConsole).Assembly.GetTypes().ToList();

            types.AddRange(typeof(CommandUtils).Assembly.GetTypes());

            foreach (var type in types)
            {
                if (!type.IsEnum || FormattingEnumCommand.EnumTypes.Any(t => t.Name == type.Name))
                    continue;

                FormattingEnumCommand.EnumTypes.Add(type);
            }
        }

        internal enum StringPart
        {
            None,
            Parameter,
            QuotedParameter
        }

        public static Dictionary<Type, ICommandParser> Parsers { get; } = new Dictionary<Type, ICommandParser>()
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
            [typeof(PlayerList)] = new PlayerListParser()
        };

        public static Dictionary<char, char> Quotes { get; } = new Dictionary<char, char>()
        {
            ['\''] = '\''
        };

        public static bool TryGetParser(Type type, out ICommandParser parser)
            => Parsers.TryGetValue(type, out parser);

        public static bool TryParseArgs(string arg, CommandInfo command, object[] parsedArgs, out ICommandArgument failedArg, out string failedReason)
        {
            _marker.MarkStart(command.Command);

            failedArg = null;
            failedReason = null;

            if (command.Arguments.Length == 1 && command.Arguments[0].Type == typeof(string))
            {
                parsedArgs[1] = arg.Trim();
                _marker.MarkEnd();
                return true;
            }

            var curArg = default(ICommandArgument);
            var curBuilder = StringBuilderPool.Shared.Rent();
            var curPart = StringPart.None;

            var argList = ListPool<object>.Shared.Rent();

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

                if (curArg != null && (curArg.IsRemainder || ((argList.Count + 1) >= command.Arguments.Length)) && i != endIndex)
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

                if (curChar == '\\' && (curArg is null || !(curArg.IsRemainder || ((argList.Count + 1) >= command.Arguments.Length))))
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
                        ListPool<object>.Shared.Return(argList);
                        StringBuilderPool.Shared.Return(curBuilder);

                        failedArg = curArg;
                        failedReason = "There must be at least one character of whitespace between arguments.";

                        _marker.MarkEnd();
                        return false;
                    }
                    else
                    {
                        if (curArg is null)
                            curArg = command.Arguments.Length > argList.Count ? command.Arguments[argList.Count] : null;

                        if (curArg != null && (curArg.IsRemainder || ((argList.Count + 1) >= command.Arguments.Length)))
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

                    if (!curArg.Parser.TryParse(argString, out var failureMessage, out var result))
                    {
                        ListPool<object>.Shared.Return(argList);
                        StringBuilderPool.Shared.Return(curBuilder);

                        failedReason = failureMessage;
                        failedArg = curArg;

                        _marker.MarkEnd();
                        return false;
                    }

                    curPart = StringPart.None;
                    curBuilder.Clear();
                    curArg = null;

                    argList.Add(result);
                }
            }

            if (curArg != null && (curArg.IsRemainder || ((argList.Count + 1) >= command.Arguments.Length)))
            {
                if (!curArg.Parser.TryParse(curBuilder.ToString(), out var failureMessage, out var remainderResult))
                {
                    ListPool<object>.Shared.Return(argList);
                    StringBuilderPool.Shared.Return(curBuilder);

                    failedReason = failureMessage;
                    failedArg = curArg;

                    _marker.MarkEnd();
                    return false;
                }

                argList.Add(remainderResult);
                curArg = null;
            }

            if (isEscaping)
            {
                ListPool<object>.Shared.Return(argList);
                StringBuilderPool.Shared.Return(curBuilder);

                failedArg = curArg;
                failedReason = "Input text may not end on an incomplete escape.";

                _marker.MarkEnd();
                return false;
            }

            if (curPart is StringPart.QuotedParameter)
            {
                ListPool<object>.Shared.Return(argList);
                StringBuilderPool.Shared.Return(curBuilder);

                failedArg = curArg;
                failedReason = "A quoted parameter is incomplete.";

                _marker.MarkEnd();
                return false;
            }

            for (int i = argList.Count; i < command.Arguments.Length; i++)
            {
                curArg = command.Arguments[i];

                if (!curArg.IsOptional)
                {
                    ListPool<object>.Shared.Return(argList);
                    StringBuilderPool.Shared.Return(curBuilder);

                    failedArg = curArg;
                    failedReason = "The input text has too few parameters.";

                    _marker.MarkEnd();
                    return false;
                }

                argList.Add(curArg.DefaultValue);
            }

            for (int i = 0; i < argList.Count; i++)
                parsedArgs[i + 1] = argList[i];

            ListPool<object>.Shared.Return(argList);
            StringBuilderPool.Shared.Return(curBuilder);

            failedArg = null;
            failedReason = null;

            _marker.MarkEnd();
            return true;
        }

        internal static bool IsOpen(char character)
            => Quotes.ContainsKey(character) || character == '\"';

        internal static char GetMatch(char character)
            => Quotes.TryGetValue(character, out var match) ? match : '\"';
    }
}