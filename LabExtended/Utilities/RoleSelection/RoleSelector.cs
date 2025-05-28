using LabExtended.API;

using LabExtended.Utilities.RoleSelection.Humans;
using LabExtended.Utilities.RoleSelection.Scps;

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

        for (var i = 0; i < teamQueue.Length; i++)
        {
            var teamChar = teamQueue[i];
            
            if (!byte.TryParse(teamChar.ToString(), out var teamId)
                || !Enum.IsDefined(typeof(Team), teamId))
                continue;

            var team = (Team)teamId;
            
            if (team != Team.SCPs)
                totalTeamQueue[totalQueueIndex++] = team;
            
            humanTeamQueue[humanQueueIndex++] = team;
        }

        var spawnableScpCount = ScpSpawner.MaxSpawnableScps;
        var spawnScpCount = 0;

        var validPlayerCount = source.Count(predicate);

        for (var i = 0; i < validPlayerCount; i++)
        {
            if (totalTeamQueue[i % totalQueueIndex] == Team.SCPs)
            {
                spawnScpCount++;

                if (spawnScpCount == spawnableScpCount && (options & RoleSelectorOptions.AllowScpOverflow) != RoleSelectorOptions.AllowScpOverflow)
                {
                    selectionResult.ScpOverflowHumeShieldMultiplier = 1f + (validPlayerCount - i) * 0.05f;
                    selectionResult.ScpsOverflowing = true;

                    break;
                }
            }
        }

        var selectionContext =
            new RoleSelectorContext(target, source, predicate, options, humanTeamQueue, totalTeamQueue);
        
        ScpRoleSelector.SelectRoles(selectionContext, spawnScpCount);
        HumanRoleSelector.SelectRoles(selectionContext, humanQueueIndex);
        
        return selectionResult;
    }
}