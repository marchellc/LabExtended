using LabExtended.API;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.RoleSelection.Humans;
using LabExtended.Utilities.RoleSelection.Scps;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.RoleAssign;

namespace LabExtended.Utilities.RoleSelection;

/// <summary>
/// Role selection utilities.
/// </summary>
public static class RoleSelector
{
    private static Func<ExPlayer, bool>? defaultPredicate;
    
    /// <summary>
    /// The default team queue.
    /// </summary>
    public const string DefaultTeamQueue = "4014314031441404134041434414";

    /// <summary>
    /// Selects roles for players.
    /// </summary>
    /// <param name="target">The target result dictionary.</param>
    /// <param name="source">The source list of players.</param>
    /// <param name="selectionPredicate">The predicate used to filter players.</param>
    /// <param name="options">The selection options.</param>
    /// <param name="teamQueue">The selection team queue.</param>
    /// <returns>Results of the role selection.</returns>
    public static RoleSelectorResult SelectRoles(this IDictionary<ExPlayer, RoleTypeId> target, List<ExPlayer> source,
        Func<ExPlayer, bool>? selectionPredicate, RoleSelectorOptions options, string teamQueue = DefaultTeamQueue)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (defaultPredicate is null)
            defaultPredicate = player => player != null && !player.IsUnverified && !target.ContainsKey(player);

        if (selectionPredicate is null)
            selectionPredicate = player =>
                player?.ReferenceHub != null && RoleAssigner.CheckPlayer(player.ReferenceHub);
        
        Func<ExPlayer, bool> predicate = player => defaultPredicate(player) && selectionPredicate(player);

        var selectionResult = new RoleSelectorResult();

        var humanTeamQueue = new Team[teamQueue.Length];
        var totalTeamQueue = new Team[teamQueue.Length];

        var totalQueueIndex = 0;
        var humanQueueIndex = 0;
        
        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Selecting roles for &6{source.Count}&r player(s):\n" +
                                      $"&3Options&r = &6{options}&r\n" +
                                      $"&3Queue&r= &6{teamQueue}&r");
        
        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Processing team queue");

        for (var i = 0; i < teamQueue.Length; i++)
        {
            var teamChar = teamQueue[i];
            
            if (!byte.TryParse(teamChar.ToString(), out var teamId)
                || !Enum.IsDefined(typeof(Team), teamId))
                continue;

            var team = (Team)teamId;
            
            if (team != Team.SCPs)
                humanTeamQueue[humanQueueIndex++] = team;
            
            totalTeamQueue[totalQueueIndex++] = team;
            
            ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Loaded team &3{team}&r (Total=[{totalQueueIndex}/{totalTeamQueue.Length}]; Human=[{humanQueueIndex}/{humanTeamQueue.Length}])");
        }

        var spawnableScpCount = ScpSpawner.MaxSpawnableScps;
        var spawnScpCount = 0;

        var validPlayerCount = source.Count(predicate);
        
        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Processing SCP overflow (SpawnableScps={spawnableScpCount}; ValidPlayerCount={validPlayerCount})");

        for (var i = 0; i < validPlayerCount; i++)
        {
            if (totalTeamQueue[i % totalQueueIndex] == Team.SCPs)
            {
                spawnScpCount++;

                if (spawnScpCount == spawnableScpCount && (options & RoleSelectorOptions.AllowScpOverflow) != RoleSelectorOptions.AllowScpOverflow)
                {
                    selectionResult.ScpOverflowHumeShieldMultiplier = 1f + (validPlayerCount - i) * 0.05f;
                    selectionResult.ScpsOverflowing = true;

                    ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r SCPs overflowing: {selectionResult.ScpOverflowHumeShieldMultiplier}");
                    break;
                }
            }
        }
        
        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r SCP count: {spawnScpCount}");

        var selectionContext =
            new RoleSelectorContext(target, source, predicate, options, humanTeamQueue, totalTeamQueue);

        if (spawnScpCount > 0)
        {
            ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Selecting SCP players");

            ScpRoleSelector.SelectRoles(selectionContext, spawnScpCount);
        }

        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Selecting human players");
        
        HumanRoleSelector.SelectRoles(selectionContext, humanQueueIndex);
        
        ApiLog.Debug("Role Selector", $"&3[SelectRoles]&r Selected roles ({target.Count}):\n{StringBuilderPool.Shared.BuildString(x =>
        {
            foreach (var role in target)
            {
                x.AppendLine($"&3{role.Key.Nickname}&r (&6{role.Key.UserId}&r): &6{role.Value}&r");
            }
        })}");
        
        return selectionResult;
    }
}