using LabExtended.Attributes;
using LabExtended.Commands.Parameters.Parsers.Wrappers;
using LabExtended.Extensions;
using LabExtended.Core;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Parameters;

using Contexts;
using Parsers;

/// <summary>
/// Used to manage argument parsing.
/// </summary>
public static class CommandParameterParserUtils
{ 
    /// <summary>
    /// Gets a list of all registered parsers.
    /// </summary>
    public static Dictionary<Type, CommandParameterParser> Parsers { get; } = new()
    {
        [typeof(string)] = new StringParameterParser()
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
    /// Parses command tokens into parameter values.
    /// </summary>
    /// <param name="parserResults">The parser's results.</param>
    /// <param name="context">The target context.</param>
    public static CommandParameterParserResult ParseParameters(CommandContext context, List<CommandParameterParserResult> parserResults)
    {
        if (parserResults is null)
            throw new ArgumentNullException(nameof(parserResults));

        if (context.Tokens.Count < context.Overload.RequiredParameters)
            return new(false, null, "MISSING_ARGS");
        
        if (context.Tokens.Count < 1)
            return new(true, null, null);
        
        ApiLog.Debug("CommandParameterParserUtils", $"Parsing overload &1{context.Overload.Name}&r with &6{context.Overload.Parameters.Count}&r parameters " +
                                                    $"(from method &6{context.Overload.Target.GetMemberName()}&r)" +
                                                    $"with &6{context.Tokens.Count}&r token(s)");

        for (var i = 0; i < context.Tokens.Count; i++)
            ApiLog.Debug("CommandParameterParserUtils", $"&3{i}&r = &6{context.Tokens[i].GetType().Name}");
        
        for (var i = 0; i < context.Overload.ParameterCount; i++)
        {
            var parameter = context.Overload.Parameters[i];

            try
            {
                if (i < context.Tokens.Count)
                {
                    var parameterToken = context.Tokens[i];
                    
                    if (!parameter.Type.Parser.AcceptsToken(parameterToken))
                    {
                        parserResults.Add(new(false, null,
                            $"Token {parameterToken.GetType().Name} is not acceptable.", parameter));
                    }
                    else
                    {
                        parserResults.Add(parameter.Type.Parser.Parse(context.Tokens, parameterToken, i, context, parameter));
                    }
                }
                else
                {
                    if (!parameter.HasDefault)
                    {
                        parserResults.Add(new(false, null, "MISSING_ARGS", parameter));
                    }
                    else
                    {
                        parserResults.Add(new(true, parameter.DefaultValue, null, parameter));
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Command Parameter Parser", $"Caught an exception while parsing parameter &1{parameter.Name}&r " +
                                                         $"in command &1{context.Command.Name}&r!\n{ex.ToColoredString()}");
                
                parserResults.Add(new(false, null, ex.Message, parameter));
            }
        }

        return new(true, null, null);
    }

    [LoaderInitialize(10)]
    private static void OnInit()
    {
        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            if (!type.IsEnum)
                                continue;
                            
                            if (Parsers.ContainsKey(type))
                                continue;
                            
                            Parsers.Add(type, new EnumParameterParser(type));
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}