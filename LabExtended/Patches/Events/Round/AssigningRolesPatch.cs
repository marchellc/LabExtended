using GameCore;

using HarmonyLib;

using PlayerRoles.RoleAssign;
using PlayerRoles;

using LabExtended.Core.Pooling.Pools;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Utilities;

namespace LabExtended.Patches.Events.Round;

/// <summary>
/// Implements the <see cref="ExRoundEvents.AssigningRoles"/> event.
/// </summary>
public static class AssigningRolesPatch
{
    // [HarmonyPatch(typeof(RoleAssigner), nameof(RoleAssigner.OnRoundStarted))]
    private static bool Prefix()
    {
        var roles = DictionaryPool<ExPlayer, RoleTypeId>.Shared.Rent();

        RoleSelector.GetRolesNonAlloc(roles,
            ConfigFile.ServerConfig.GetBool("allow_scp_overflow"),
            true, true, ConfigFile.ServerConfig.GetString("team_respawn_queue", "4014314031441404134041434414"));

        if (!ExRoundEvents.OnAssigningRoles(new(roles)))
            return false;

        foreach (var pair in roles)
            pair.Key.Role.Set(pair.Value, RoleChangeReason.RoundStart, RoleSpawnFlags.All);

        DictionaryPool<ExPlayer, RoleTypeId>.Shared.Return(roles);
        return false;
    }
}