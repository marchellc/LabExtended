using LabExtended.API;

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
        using (var tickerLoader = new ScpTicketsLoader())
        {
            GenerateScps(context, scpContext, tickerLoader, scpCount);

            if (!context.HasOption(RoleSelectorOptions.ModifyScpTickets))
                return;

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
            return;

        var ticketCount = 0;

        for (var i = 0; i < context.Players.Count; i++)
        {
            var player = context.Players[i];
            
            if (!context.Predicate(player))
                continue;

            var tickets = loader.GetTickets(player.ReferenceHub, 10);

            if (tickets >= ticketCount)
            {
                if (tickets > ticketCount)
                    scpContext.Chosen.Clear();

                ticketCount = tickets;
                
                scpContext.Chosen.Add(player);
            }
        }

        if (scpContext.Chosen.Count > 1)
        {
            var randomPlayer = scpContext.Chosen.RandomItem();
            
            scpContext.Chosen.Clear();
            scpContext.Chosen.Add(randomPlayer);
        }

        scpCount -= scpContext.Chosen.Count;

        if (scpCount < 1)
            return;

        var potentialScps = ListPool<KeyValuePair<ExPlayer, long>>.Shared.Rent();
        var weight = 0L;

        for (var i = 0; i < context.Players.Count; i++)
        {
            var player = context.Players[i];

            if (!scpContext.Chosen.Contains(player) && context.Predicate(player))
            {
                var playerWeight = 1L;
                var playerTickets = loader.GetTickets(player.ReferenceHub, 10);

                for (var x = 0; x < scpCount; x++)
                    playerWeight *= playerTickets;
                
                potentialScps.Add(new(player, playerWeight));
                
                weight += playerWeight;
            }
        }

        while (scpCount > 0)
        {
            var randomWeight = weight * UnityEngine.Random.value;

            for (var i = 0; i < potentialScps.Count; i++)
            {
                var pair = potentialScps[i];

                randomWeight -= pair.Value;

                if (randomWeight <= 0.0)
                {
                    scpCount--;

                    scpContext.Chosen.Add(pair.Key);

                    potentialScps.RemoveAt(i);

                    weight -= pair.Value;
                    break;
                }
            }
        }

        ListPool<KeyValuePair<ExPlayer, long>>.Shared.Return(potentialScps);
    }
}