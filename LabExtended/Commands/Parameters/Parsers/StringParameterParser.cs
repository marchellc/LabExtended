using LabExtended.Commands.Tokens;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parses string parameters.
/// </summary>
public class StringParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "A word";

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex,
        CommandContext context, CommandParameter parameter)
    {
        if (token is StringToken stringToken)
            return new(true, stringToken.Value, null, parameter);
        
        if (token is PropertyToken propertyToken && propertyToken.TryGet<object>(context, null, out var result))
            return new(true, result.ToString(), null, parameter);

        return new(false, null, $"Unsupported token: {token.GetType().Name}", parameter);
    }
}