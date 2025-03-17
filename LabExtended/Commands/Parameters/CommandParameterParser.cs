using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents a parameter parser.
/// </summary>
public class CommandParameterParser
{
    /// <summary>
    /// Gets a list of all registered parsers.
    /// </summary>
    public static Dictionary<Type, CommandParameterParser> Parsers { get; } = new();
    
    /// <summary>
    /// Attempts to find a suitable parameter parser.
    /// </summary>
    /// <param name="parameterType">The type to find the parser for.</param>
    /// <param name="parser">The found parser instance.</param>
    /// <returns>true if the parser was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetParser(Type parameterType, out CommandParameterParser parser)
    {
        if (parameterType is null)
            throw new ArgumentNullException(nameof(parameterType));
        
        return Parsers.TryGetValue(parameterType, out parser);
    }

    /// <summary>
    /// Parses command tokens into parameter values.
    /// </summary>
    /// <param name="overload">The overload to parse.</param>
    /// <param name="parsedTokens">The parsed tokens.</param>
    /// <param name="parserResults">The parser's results.</param>
    /// <param name="context">The target context.</param>
    public static void ParseParameters(CommandOverload overload, List<ICommandToken> parsedTokens,
        List<CommandParameterParserResult> parserResults, ref CommandContext context)
    {
        if (overload is null)
            throw new ArgumentNullException(nameof(overload));
        
        if (parsedTokens is null)
            throw new ArgumentNullException(nameof(parsedTokens));
        
        if (parserResults is null)
            throw new ArgumentNullException(nameof(parserResults));

        if (parsedTokens.Count < 1)
            return;

        if (parsedTokens.Count < overload.RequiredParameters)
            throw new Exception($"Not enough tokens");
        
        if (parserResults.Capacity < overload.Parameters.Count)
            parserResults.Capacity = overload.Parameters.Count;
        
        for (int i = 0; i < overload.Parameters.Count; i++)
        {
            var parameter = overload.Parameters[i];

            try
            {
                if (i < parsedTokens.Count)
                {
                    var parameterToken = parsedTokens[i];

                    if (!parameter.AcceptsToken(parameterToken))
                        throw new Exception($"Token {parameterToken} is not acceptable by parameter");

                    parserResults.Add(parameter.Parser.Parse(parsedTokens, parameterToken, i, ref context));
                }
                else
                {
                    if (!parameter.HasDefault)
                        throw new Exception($"Not enough tokens");

                    parserResults.Add(new(true, parameter.DefaultValue, null));
                }
            }
            catch (Exception ex)
            {
                parserResults.Add(new(false, null, ex.Message));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether or not the token can be accepted by this parameter.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>true if the token is acceptable</returns>
    public bool AcceptsToken(ICommandToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        return true;
    }

    /// <summary>
    /// Parses a token into an argument.
    /// </summary>
    /// <param name="tokens">All parsed tokens.</param>
    /// <param name="token">The current token to parse.</param>
    /// <param name="tokenIndex">The index of the current token.</param>
    /// <param name="context">The command context.</param>
    /// <returns>The result of parsing.</returns>
    public CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, ref CommandContext context)
    {
        return default;
    }
}