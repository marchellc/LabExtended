using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;
using LabExtended.Core;

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
            ApiLog.Debug("PlayerParameterParser", $"token is PropertyToken");
            
            if (propertyToken.TryProcessProperty(context, out var result))
            {            
                ApiLog.Debug("PlayerParameterParser", $"Processed property, result: {result}");

                if (result is ExPlayer)
                {
                    ApiLog.Debug("PlayerParameterParser", $"Result is ExPlayer");
                    return new(true, result, null, parameter);
                }

                sourceString = result.ToString();
            }
        }
        else if (token is StringToken stringToken)
        {
            sourceString = stringToken.Value;
        }
        
        ApiLog.Debug("PlayerParameterParser", $"sourceString: {sourceString}");
        
        if (ExPlayer.TryGet(sourceString, 0.85, out var player))
            return new(true, player, null, parameter);
        
        return new(false, null, $"Could not find player \"{sourceString}\"", parameter);
    }
}