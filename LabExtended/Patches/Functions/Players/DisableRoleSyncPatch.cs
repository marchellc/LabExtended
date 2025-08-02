using HarmonyLib;

using PlayerRoles;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Prevents the base-game from synchronizing roles.
/// </summary>
public static class DisableRoleSyncPatch
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.SendNewRoleInfo))]
    private static bool Prefix(PlayerRoleManager __instance) => false;
}