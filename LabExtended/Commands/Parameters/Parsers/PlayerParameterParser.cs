using LabExtended.Commands.Tokens;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters.Restrictions;

namespace LabExtended.Commands.Parameters.Parsers;

using API;

/// <summary>
/// Parses <see cref="ExPlayer"/>.
/// </summary>
public class PlayerParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "Player Nick / Player ID / User ID";

    /// <inheritdoc cref="CommandParameterParser.UsageAlias"/>
    public override string? UsageAlias { get; } = "%player%";

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        var sourceString = string.Empty;
        
        if (token is PropertyToken propertyToken
            && propertyToken.TryGet<ExPlayer>(context, null, out var player))
            return new(true, player, null, parameter);
        
        if (token is StringToken stringToken)
            sourceString = stringToken.Value;
        else
            return new(false, null, $"Unsupported token: {token.GetType().Name}", parameter);

        var precision = 0.85;
        
        if (parameter.HasRestriction<PlayerPrecisionRestriction>(out var precisionRestriction))
            precision = precisionRestriction.Precision;
        
        if (ExPlayer.TryGet(sourceString, precision, out player))
            return new(true, player, null, parameter);
        
        return new(false, null, $"Could not find player \"{sourceString}\"", parameter);
    }
}