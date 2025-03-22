using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parses string parameters.
/// </summary>
public class StringParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex,
        CommandContext context, CommandParameter parameter)
    {
        if (token is StringToken stringToken)
            return new(true, stringToken.Value, null);

        return new(false, null, $"Unsupported token: {token.GetType().Name}");
    }
}