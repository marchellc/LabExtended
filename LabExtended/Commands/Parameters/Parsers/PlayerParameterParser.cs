using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Parsers;

using API;

/// <summary>
/// Parses <see cref="ExPlayer"/>.
/// </summary>
public class PlayerParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "Player Nick / Player ID / User ID";

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        var sourceString = string.Empty;
        
        if (token is PropertyToken propertyToken)
        {
            if (propertyToken.TryProcessProperty(context, out var result))
            {
                if (result is ExPlayer)
                    return new(true, result, null, parameter);

                sourceString = result.ToString();
            }
            else if (propertyToken.Key == "context" && propertyToken.Name == "sender")
            {
                return new(true, context.Sender, null, parameter);
            }
        }
        else if (token is StringToken stringToken)
        {
            sourceString = stringToken.Value;
        }
        
        if (ExPlayer.TryGet(sourceString, 0.85, out var player))
            return new(true, player, null, parameter);
        
        return new(false, null, $"Could not find player \"{sourceString}\"", parameter);
    }
}