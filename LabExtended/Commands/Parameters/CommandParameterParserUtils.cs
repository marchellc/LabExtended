using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Tokens;

using NorthwoodLib.Pools;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.Commands.Parameters;

using API;
using Core;
using Utilities;
using Extensions;

using Parsers;
using Parsers.Wrappers;

using LabExtended.Utilities;

/// <summary>
/// Used to manage argument parsing.
/// </summary>
public static class CommandParameterParserUtils
{
    // Used only for non-command overload parsing!
    private static readonly CommandParameter NullParameter = new();
    
    /// <summary>
    /// Gets a list of all registered parsers.
    /// </summary>
    public static Dictionary<Type, CommandParameterParser> Parsers { get; } = new()
    {
        [typeof(char)] = new CharParameterParser(),
        [typeof(string)] = new StringParameterParser(),
        
        [typeof(bool)] = new DelegateParameterParser<bool>(CommandDelegateParsers.TryParseBool, "Boolean (true / false)"),
        
        [typeof(byte)] = new DelegateParameterParser<byte>(CommandDelegateParsers.TryParseByte, $"A number ranging from {byte.MinValue} to {byte.MaxValue}"),
        [typeof(sbyte)] = new DelegateParameterParser<sbyte>(CommandDelegateParsers.TryParseSByte, $"A number ranging from {sbyte.MinValue} to {sbyte.MaxValue}"),
        
        [typeof(short)] = new DelegateParameterParser<short>(CommandDelegateParsers.TryParseShort, $"A number ranging from {short.MinValue} to {short.MaxValue}"),
        [typeof(ushort)] = new DelegateParameterParser<ushort>(CommandDelegateParsers.TryParseUShort, $"A number ranging from {ushort.MinValue} to {ushort.MaxValue}"),
        
        [typeof(int)] = new DelegateParameterParser<int>(CommandDelegateParsers.TryParseInt, $"A number ranging from {int.MinValue} to {int.MaxValue}"),
        [typeof(uint)] = new DelegateParameterParser<uint>(CommandDelegateParsers.TryParseUInt, $"A number ranging from {uint.MinValue} to {uint.MaxValue}"),
        
        [typeof(long)] = new DelegateParameterParser<long>(CommandDelegateParsers.TryParseLong, $"A number ranging from {long.MinValue} to {long.MaxValue}"),
        [typeof(ulong)] = new DelegateParameterParser<ulong>(CommandDelegateParsers.TryParseULong, $"A number ranging from {ulong.MinValue} to {ulong.MaxValue}"),
        
        [typeof(float)] = new DelegateParameterParser<float>(CommandDelegateParsers.TryParseFloat, $"A number ranging from {float.MinValue} to {float.MaxValue}"),
        [typeof(double)] = new DelegateParameterParser<double>(CommandDelegateParsers.TryParseDouble, $"A number ranging from {double.MinValue} to {double.MaxValue}"),
        [typeof(decimal)] = new DelegateParameterParser<decimal>(CommandDelegateParsers.TryParseDecimal, $"A number ranging from {decimal.MinValue} to {decimal.MaxValue}"),
        
        [typeof(DateTime)] = new DelegateParameterParser<DateTime>(CommandDelegateParsers.TryParseDate, "A date."),
        
        [typeof(TimeSpan)] = new TimeSpanParameterParser(),
        [typeof(Color)] = new ColorParameterParser(),
        
        [typeof(Quaternion)] = new QuaternionParameterParser(),
        [typeof(Vector3)] = new Vector3ParameterParser(),
        [typeof(Vector2)] = new Vector2ParameterParser(),
        
        [typeof(ExPlayer)] = new PlayerParameterParser(),
        [typeof(List<ExPlayer>)] = new PlayerListParameterParser()
    };

    /// <summary>
    /// Attempts to find a suitable parameter parser.
    /// </summary>
    /// <param name="type">The type to find the parser for.</param>
    /// <param name="parser">The found parser instance.</param>
    /// <returns>true if the parser was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetParser(Type type, out CommandParameterParser parser)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        var nullableType = Nullable.GetUnderlyingType(type);
        
        if (nullableType != null)
            type = nullableType;
        
        if (Parsers.TryGetValue(type, out parser))
            return true;

        if (type.IsEnum)
        {
            parser = new EnumParameterParser(type);
            
            Parsers.Add(type, parser);
            return true;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            
            parser = new ArrayWrapperParser(parser, elementType);
            
            Parsers.Add(type, parser);
            return true;
        }

        if (type.IsConstructedGenericType)
        {
            var genericDefinition = type.GetGenericTypeDefinition();
            var genericArgs = type.GetGenericArguments();

            if (genericDefinition != null)
            {
                if (genericDefinition == typeof(List<>))
                {
                    var elementType = genericArgs[0];
                    
                    if (!TryGetParser(elementType, out var elementParser))
                        return false;
                    
                    parser = new ListWrapperParser(elementParser, type);
                    
                    Parsers.Add(type, parser);
                    return true;
                }

                if (genericDefinition == typeof(HashSet<>))
                {
                    var elementType = genericArgs[0];
                    
                    if (!TryGetParser(elementType, out var elementParser))
                        return false;
                    
                    parser = new HashSetWrapperParser(elementParser, type);
                    
                    Parsers.Add(type, parser);
                    return true;
                }

                if (genericDefinition == typeof(Dictionary<,>))
                {
                    var keyType = genericArgs[0];
                    var valueType = genericArgs[1];

                    if (!TryGetParser(keyType, out var keyParser))
                        return false;
                    
                    if (!TryGetParser(valueType, out var valueParser))
                        return false;
                    
                    parser = new DictionaryWrapperParser(type, keyParser, valueParser);
                    
                    Parsers.Add(type, parser);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Compiles a list of parameters into a string array.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <returns>The compiled parameters array.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string[] CompileParameters(this IEnumerable<CommandParameter> parameters)
    {
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters));

        var list = ListPool<string>.Shared.Rent();

        foreach (var parameter in parameters)
        {
            if (parameter.UsageAlias != null)
                list.Add(parameter.UsageAlias);
            else
                list.Add(parameter.Name);
        }

        return ListPool<string>.Shared.ToArrayReturn(list);
    }

    /// <summary>
    /// Attempts to parse a given string to a target value type.
    /// </summary>
    /// <param name="context">The command's execution context.</param>
    /// <param name="value">The string to parse.</param>
    /// <param name="parserResult">The parsing result of the parser.</param>
    /// <typeparam name="T">The type to parse.</typeparam>
    /// <returns>true if the parser was found, otherwise false</returns>
    public static bool TryParse<T>(this CommandContext context, string value, out CommandParameterParserResult parserResult)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (!TryGetParser(typeof(T), out var parser))
        {
            parserResult = new(false, null, $"Missing parser for type '{typeof(T).Name}'", null, null);
        }
        else
        {
            var token = StringToken.Instance.NewToken<StringToken>();
            var tokens = ListPool<ICommandToken>.Shared.Rent();

            tokens.Add(token);

            token.Value = value;

            parserResult = parser.Parse(tokens, token, 0, context, NullParameter);

            token.ReturnToken();

            ListPool<ICommandToken>.Shared.Return(tokens);
        }

        return parserResult.Success;
    }
    
    /// <summary>
    /// Parses command tokens into parameter values.
    /// </summary>
    /// <param name="parserResults">The parser's results.</param>
    /// <param name="context">The target context.</param>
    public static CommandParameterParserResult ParseParameters(CommandContext context, List<CommandParameterParserResult> parserResults)
    {
        if (parserResults is null)
            throw new ArgumentNullException(nameof(parserResults));

        if (context.Tokens.Count < context.Overload.RequiredParameters)
            return new(false, null, "MISSING_ARGS", null, null!);

        if (context.Tokens.Count < 1)
        {
            context.Overload.Parameters.ForEach(p => parserResults.Add(new(true, p.DefaultValue, null, p, null!)));
            return new(true, null, null, null, null!);
        }

        var index = 0;

        context.Overload.Parameters.ForEach(parameter =>
        {
            try
            {
                if (index < context.Tokens.Count)
                {
                    var parameterToken = context.Tokens[index];
                    var parserResult = new CommandParameterParserResult(false, null, null, null, null!);

                    foreach (var pair in parameter.Parsers)
                    {
                        if (!pair.Key.AcceptsToken(parameterToken))
                            continue;

                        parserResult = pair.Key.Parse(context.Tokens, parameterToken, index, context, parameter);

                        if (parserResult.Success)
                            break;
                    }

                    if (parserResult.Success)
                    {
                        string? argumentError = null;

                        foreach (var restriction in parameter.Restrictions)
                        {
                            if (!restriction.IsValid(parserResult.Value!, context, parameter, out var error))
                            {
                                argumentError = error;
                                break;
                            }
                        }

                        if (argumentError != null)
                        {
                            parserResults.Add(new(false, null, argumentError, parameter, null!));

                            index++;
                            return;
                        }
                    }

                    parserResults.Add(parserResult);
                }
                else
                {
                    if (!parameter.HasDefault)
                    {
                        parserResults.Add(new(false, null, "MISSING_ARGS", parameter, null!));

                        index++;
                        return;
                    }

                    parserResults.Add(new(true, parameter.DefaultValue, null, parameter, null!));
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Command Parameter Parser",
                    $"Caught an exception while parsing parameter &1{parameter.Name}&r " +
                    $"in command &1{context.Command.Name}&r!\n{ex.ToColoredString()}");

                parserResults.Add(new(false, null, ex.Message, parameter, null!));
            }

            index++;
        });

        return new(true, null, null, null, null!);
    }

    private static void Internal_Discovered(Type type)
    {
        if (!type.IsEnum)
            return;

        if (Parsers.ContainsKey(type))
            return;

        Parsers.Add(type, new EnumParameterParser(type));
    }

    internal static void Internal_Init()
    {
        ReflectionUtils.Discovered += Internal_Discovered;
    }
}