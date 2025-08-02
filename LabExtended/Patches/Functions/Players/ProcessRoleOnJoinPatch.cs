using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RoleSync;

using Mirror;

using PlayerRoles;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Prevents faked roles from blinking in the spectator list by changing the initial sync packet.
/// </summary>
public static class ProcessRoleOnJoinPatch
{
    [HarmonyPatch(typeof(RoleSyncInfoPack), nameof(RoleSyncInfoPack.WritePlayers))]
    private static bool Prefix(RoleSyncInfoPack __instance, NetworkWriter writer)
    {
        if (__instance._receiverHub is null
            || !ExPlayer.TryGet(__instance._receiverHub, out var receiver))
            return true;

        RoleManager.Internal_ProcessJoin(receiver, writer);
        return false;
    }
}
