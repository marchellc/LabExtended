using HarmonyLib;

using PlayerRoles;

namespace LabExtended.Patches.Functions.SpectatorList
{
    public static class SpectatorListSyncRolePatch
    {
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Update))]
        public static bool Prefix(PlayerRoleManager __instance)
            => false;
    }
}