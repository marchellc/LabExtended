using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Commands.Parameters.Restrictions;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Tokens;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// A parser that targets a <see cref="List{T}"/> of players.
/// </summary>
public class PlayerListParameterParser : CommandParameterParser
{
    // * - All players
    // *! - All players but sender
    
    // *& - All dead players
    // *!& - ALl dead players but sender
    
    // *&! - All alive players
    // *!&! - All alive players but sender
    
    // *| - All staff players
    // *!| - All staff players but sender
    
    // *|! - All non-staff players
    // *!|! - All non-staff players but sender
    
    /// <summary>
    /// A list of custom player list token handles.
    /// <remarks>A handle must start with *</remarks>
    /// </summary>
    public static Dictionary<string, Action<CommandContext, List<ExPlayer>>> TokenHandles { get; } = new()
    {
        ["*"] = (_, list) => list.AddRange(ExPlayer.Players),
        ["*!"] = (context, list) => list.AddRangeWhere(ExPlayer.Players, p => p != context.Sender),
        
        ["*&"] = (_, list) => list.AddRangeWhere(ExPlayer.Players, p => p.Role.IsDead),
        ["*!&"] = (context, list) => list.AddRangeWhere(ExPlayer.Players, p => p != context.Sender && p.Role.IsDead),
        
        ["*&!"] = (_, list) => list.AddRangeWhere(ExPlayer.Players, p => p.Role.IsAlive),
        ["*!&!"] = (context, list) => list.AddRangeWhere(ExPlayer.Players, p => p != context.Sender && p.Role.IsAlive),
        
        ["*|"] = (_, list) => list.AddRangeWhere(ExPlayer.Players, p => p.HasRemoteAdminAccess),
        ["*!|"] = (context, list) => list.AddRangeWhere(ExPlayer.Players, p => p != context.Sender && p.HasRemoteAdminAccess),
        
        ["*|!"] = (_, list) => list.AddRangeWhere(ExPlayer.Players, p => !p.HasRemoteAdminAccess),
        ["*!|!"] = (context, list) => list.AddRangeWhere(ExPlayer.Players, p => p != context.Sender && !p.HasRemoteAdminAccess),
    };
    
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "A list of player names, user IDs, player IDs or IPs.";

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex,
        CommandContext context, CommandParameter parameter)
    {
        var list = default(List<string>);
        var listReturn = false;

        if (token is StringToken stringToken)
        {
            if (stringToken.Value.StartsWith("*") && TokenHandles.TryGetValue(stringToken.Value, out var tokenHandle))
            {
                var players = new List<ExPlayer>();

                tokenHandle(context, players);
                return new(true, players, null, parameter);
            }

            list = stringToken.ToListNonAlloc();
            listReturn = true;
        }
        else if (token is CollectionToken collectionToken)
        {
            list = collectionToken.Values;
        }
        else
        {
            return new(false, null, $"Unsupported token type: {token.GetType().Name}", parameter);
        }

        var found = new List<ExPlayer>(list.Count);
        var precision = 0.85;
        
        if (parameter.HasRestriction<PlayerPrecisionRestriction>(out var precisionRestriction))
            precision = precisionRestriction.Precision;

        for (var i = 0; i < list.Count; i++)
        {
            var query = list[i];
            
            if (CommandPropertyUtils.TryGetPlayer(ref query, context, out var player) && player != null)
                found.Add(player);
            else if (ExPlayer.TryGet(query, precision, out player) && player != null)
                found.Add(player);
            else
                return new(false, null, $"Could not find player \"{query}\" (at: {i})", parameter);
        }

        if (listReturn)
            ListPool<string>.Shared.Return(list);
        
        return new(true, found, null, parameter);
    }
}