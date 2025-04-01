using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Tokens;
using LabExtended.Commands.Utilities;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parses <see cref="char"/>.
/// </summary>
public class CharParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "A singular character";
    
    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        if (token is StringToken stringToken)
            return new(true, stringToken.Value[0], null, parameter);

        if (token is PropertyToken propertyToken
            && propertyToken.TryGet<object>(context, null, out var result))
        {
            if (result is char c)
                return new(true, c, null, parameter);
            
            if (result is string str)
                return new(true, str[0], null, parameter);

            return new(false, null, $"Unsupported property type: {result.GetType().FullName}", parameter);
        }

        return new(false, null, $"Unsupported token: {token.GetType().Name}", parameter);
    }
}