using PlayerRoles;
using PlayerRoles.RoleAssign;

namespace LabExtended.Utilities.RoleSelection.Humans;

/// <summary>
/// Selects human roles.
/// </summary>
public static class HumanRoleSelector
{
    /// <summary>
    /// Selects human roles.
    /// </summary>
    /// <param name="context">The role selection context.</param>
    /// <param name="humanCount">How many roles to select.</param>
    public static void SelectRoles(RoleSelectorContext context, int humanCount)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        
        if (humanCount < 0)
            throw new ArgumentOutOfRangeException(nameof(humanCount));
        
        using (var humanContext = new HumanRoleSelectorContext())
        {
            humanContext.roleClock = 0;
            humanContext.roleLength = humanCount;

            var validCount = context.Players.Count(context.Predicate);

            humanContext.Roles = new RoleTypeId[validCount];

            for (var i = 0; i < validCount; i++)
                humanContext.Roles[i] = GetNextRole(context, humanContext);
            
            humanContext.Roles.ShuffleList();
            
            for (var i = 0; i < humanContext.Roles.Length; i++)
                AssignRole(context, humanContext, humanCount, humanContext.Roles[i]);
        }
    }

    private static void AssignRole(RoleSelectorContext context, HumanRoleSelectorContext humanContext, int humanCount,
        RoleTypeId role)
    {
        humanContext.Candidates.Clear();

        var historyCount = int.MaxValue;

        for (var i = 0; i < context.Players.Count; i++)
        {
            var player = context.Players[i];
            
            if (!context.Predicate(player))
                continue;

            var history = HumanSpawner.History.GetOrAdd(player.UserId, () => new());
            var count = 0;
            
            for (var x = 0; x < 5; x++)
            {
                if (history.History[x] == role)
                {
                    count++;
                }
            }

            if (count <= historyCount)
            {
                if (count < historyCount)
                    humanContext.Candidates.Clear();

                humanContext.Candidates.Add(player);

                historyCount = count;
            }
        }

        if (humanContext.Candidates.Count == 0)
            return;

        var randomPlayer = humanContext.Candidates.RandomItem();
        
        context.Roles[randomPlayer] = role;
        
        if (context.HasOption(RoleSelectorOptions.ModifyHumanHistory))
            HumanSpawner.History[randomPlayer.UserId].RegisterRole(role);
    }

    private static RoleTypeId GetNextRole(RoleSelectorContext context, HumanRoleSelectorContext humanContext)
    {
        var team = context.HumanQueue[humanContext.roleClock++ % humanContext.roleLength];

        if (!HumanSpawner.Handlers.TryGetValue(team, out var handler))
            return RoleTypeId.ClassD;

        return handler.NextRole;
    }
}