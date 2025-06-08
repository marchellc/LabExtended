using LabExtended.API;
using LabExtended.Core;
using NorthwoodLib.Pools;

using PlayerRoles.RoleAssign;

namespace LabExtended.Utilities.RoleSelection.Scps;

/// <summary>
/// Selects SCP role candidates.
/// </summary>
public static class ScpPlayerSelector
{
    /// <summary>
    /// Selects a list of SCP players.
    /// </summary>
    /// <param name="context">Role selection context.</param>
    /// <param name="scpContext">SCP selection context.</param>
    /// <param name="scpCount">Number of players to select.</param>
    public static void SelectPlayers(RoleSelectorContext context, ScpRoleSelectorContext scpContext, int scpCount)
    {
        ApiLog.Debug("Scp Player Selector", $"&3[SelectPlayers]&r Selecting &6{scpCount}&r players");
        
        using (var tickerLoader = new ScpTicketsLoader())
        {
            ApiLog.Debug("Scp Player Selector", $"&3[SelectPlayers]&r Generating player list");
            
            GenerateScps(context, scpContext, tickerLoader, scpCount);

            if (!context.HasOption(RoleSelectorOptions.ModifyScpTickets))
                return;
            
            ApiLog.Debug("Scp Player Selector", $"&3[SelectPlayers]&r Modifying tickets");

            for (var i = 0; i < context.Players.Count; i++)
            {
                var player = context.Players[i];
                
                if (ScpPlayerPicker.IsOptedOutOfScp(player.ReferenceHub))
                    continue;
                
                if (!context.Predicate(player))
                    continue;
                
                tickerLoader.ModifyTickets(player.ReferenceHub, tickerLoader.GetTickets(player.ReferenceHub, 10) + 2);
            }

            for (var i = 0; i < scpContext.Chosen.Count; i++)
                tickerLoader.ModifyTickets(scpContext.Chosen[i].ReferenceHub, 10);
        }
    }
    
    private static void GenerateScps(RoleSelectorContext context, ScpRoleSelectorContext scpContext, ScpTicketsLoader loader, int scpCount)
    {
        scpContext.Chosen.Clear();

        if (scpCount < 1)
        {
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r ScpCount is less than one");
            return;
        }

        var ticketCount = 0;

        for (var i = 0; i < context.Players.Count; i++)
        {
            var player = context.Players[i];
            
            if (!context.Predicate(player))
                continue;
            
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Processing player &3{player.Nickname}&r (&6{player.UserId}&r)");

            var tickets = loader.GetTickets(player.ReferenceHub, 10);

            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Tickets: {tickets} / {ticketCount}");
            
            if (tickets >= ticketCount)
            {
                if (tickets > ticketCount)
                {
                    scpContext.Chosen.Clear();
                    
                    ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Cleared the Chosen list");
                }

                ticketCount = tickets;
                
                scpContext.Chosen.Add(player);
                
                ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Added &3{player.Nickname}&r (&6{player.UserId}&r)");
            }
        }

        if (scpContext.Chosen.Count > 1)
        {
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Chosen contains more than one player");
            
            var randomPlayer = scpContext.Chosen.RandomItem();
            
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Selected random: &3{randomPlayer.Nickname}&r (&6{randomPlayer.UserId}&r)");
            
            scpContext.Chosen.Clear();
            scpContext.Chosen.Add(randomPlayer);
        }

        scpCount -= scpContext.Chosen.Count;

        ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Remaining SCPs: {scpCount}");
        
        if (scpCount < 1)
            return;

        var potentialScps = ListPool<KeyValuePair<ExPlayer, long>>.Shared.Rent();
        var weight = 0L;

        for (var i = 0; i < context.Players.Count; i++)
        {
            var player = context.Players[i];
            
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Processing potential SCP &3{player.Nickname}&r (&6{player.UserId}&r)");

            if (!scpContext.Chosen.Contains(player) && context.Predicate(player))
            {
                var playerWeight = 1L;
                var playerTickets = loader.GetTickets(player.ReferenceHub, 10);

                for (var x = 0; x < scpCount; x++)
                    playerWeight *= playerTickets;
                
                potentialScps.Add(new(player, playerWeight));
                
                weight += playerWeight;
                
                ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Player weight: &3{player.Nickname}&r (&6{player.UserId}&r) = {playerWeight} (Total: {weight})");
            }
        }

        while (scpCount > 0)
        {
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Selecting additional player ({scpCount})");
            
            var randomWeight = weight * UnityEngine.Random.value;
            
            ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Random Weight: {randomWeight}");

            for (var i = 0; i < potentialScps.Count; i++)
            {
                var pair = potentialScps[i];

                randomWeight -= pair.Value;

                if (randomWeight <= 0.0)
                {
                    scpCount--;

                    scpContext.Chosen.Add(pair.Key);

                    potentialScps.RemoveAt(i);

                    ApiLog.Debug("Scp Player Selector", $"&3[GenerateScps]&r Selected player &3{pair.Key.Nickname}&r (&6{pair.Key.UserId}&r)");
                    
                    weight -= pair.Value;
                    break;
                }
            }
        }

        ListPool<KeyValuePair<ExPlayer, long>>.Shared.Return(potentialScps);
    }
}