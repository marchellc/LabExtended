using PlayerRoles;
using PlayerRoles.PlayableScps;
using PlayerRoles.RoleAssign;

using UnityEngine;

namespace LabExtended.Utilities.RoleSelection.Scps;

/// <summary>
/// Selects SCP roles.
/// </summary>
public static class ScpRoleSelector
{
    /// <summary>
    /// Selects SCP roles.
    /// </summary>
    /// <param name="context">The target selection context.</param>
    /// <param name="scpsToSelect">The SCP role count.</param>
    public static void SelectRoles(RoleSelectorContext context, int scpsToSelect)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        
        if (scpsToSelect < 0)
            throw new ArgumentOutOfRangeException(nameof(scpsToSelect));
        
        using (var scpContext = new ScpRoleSelectorContext(context))
        {
            scpContext.Roles.Clear();

            for (var i = 0; i < scpsToSelect; i++)
                scpContext.Roles.Add(GetNextRole(context, scpContext));

            ScpPlayerSelector.SelectPlayers(context, scpContext, scpsToSelect);

            while (scpContext.Roles.Count > 0)
            {
                var role = scpContext.Roles[0];

                scpContext.Roles.RemoveAt(0);
                
                AssignRole(context, scpContext, role);
            }
        }
    }

    private static void AssignRole(RoleSelectorContext context, ScpRoleSelectorContext scpContext, RoleTypeId scpRole)
    {
        scpContext.Chances.Clear();

        var num = 1;
        var num2 = 0;

        for (var i = 0; i < scpContext.Chosen.Count; i++)
        {
            var player = scpContext.Chosen[i];
            var rolePreference = ScpSpawner.GetCombinedPreferencePoints(player.ReferenceHub, scpRole, scpContext.Roles);

            for (var x = 0; x < scpContext.Roles.Count; x++)
                rolePreference -= ScpSpawner.GetCombinedPreferencePoints(player.ReferenceHub, scpContext.Roles[x], scpContext.Roles);

            num2++;

            scpContext.Chances[player] = rolePreference;
            
            num = Mathf.Min(rolePreference, num);
        }
        
        scpContext.SelectedChances.Clear();

        var totalChance = 0f;
        
        foreach (var pair in scpContext.Chances)
        {
            var chance = Mathf.Pow(pair.Value - num + 1f, num2);

            scpContext.SelectedChances[pair.Key] = chance;

            totalChance += chance;
        }

        var randomChance = totalChance * UnityEngine.Random.value;
        var weight = 0f;

        foreach (var pair in scpContext.SelectedChances)
        {
            weight += pair.Value;

            if (pair.Value >= randomChance)
            {
                scpContext.Chosen.Remove(pair.Key);
                
                context.Roles[pair.Key] = scpRole;
                break;
            }
        }
    }

    private static RoleTypeId GetNextRole(RoleSelectorContext context, ScpRoleSelectorContext scpContext)
    {
        var totalWeight = 0f;
        var scpCount = ScpSpawner.SpawnableScps.Length;

        for (var i = 0; i < scpCount; i++)
        {
            var scpRole = ScpSpawner.SpawnableScps[i];

            if (scpContext.Roles.Contains(scpRole.RoleTypeId))
            {
                scpContext.RoleChances[i] = 0f;
                continue;
            }

            var spawnChance = Mathf.Max((scpRole as ISpawnableScp).GetSpawnChance(scpContext.Roles), 0f);
            
            totalWeight += spawnChance;
            
            scpContext.RoleChances[i] = spawnChance;
        }
        
        if (totalWeight == 0f)
            return GetRandomRole(context, scpContext);
        
        var random = UnityEngine.Random.Range(0f, totalWeight);

        for (var i = 0; i < scpCount; i++)
        {
            random -= scpContext.RoleChances[i];
            
            if (random <= 0f)
                return ScpSpawner.SpawnableScps[i].RoleTypeId;
        }

        return ScpSpawner.SpawnableScps[scpCount - 1].RoleTypeId;
    }

    private static RoleTypeId GetRandomRole(RoleSelectorContext context, ScpRoleSelectorContext scpContext)
    {
        var spawnableCount = ScpSpawner.SpawnableScps.Length;
        var scpCount = 0;

        for (var i = 0; i < spawnableCount; i++)
        {
            var scpRole = ScpSpawner.SpawnableScps[i].RoleTypeId;
            var otherOccurences = 0;

            foreach (var otherRole in scpContext.Roles)
            {
                if (otherRole != scpRole)
                    continue;
                
                otherOccurences++;
            }

            if (otherOccurences <= scpCount)
            {
                if (otherOccurences < scpCount)
                    scpContext.Backup.Clear();
                
                scpContext.Backup.Add(scpRole);
                
                scpCount = otherOccurences;
            }
        }

        return scpContext.Backup.RandomItem();
    }
}