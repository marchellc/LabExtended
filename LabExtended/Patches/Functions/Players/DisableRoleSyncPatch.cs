using HarmonyLib;

using PlayerRoles;

namespace LabExtended.Patches.Functions.Players
{
    public static class DisableRoleSyncPatch
    {
        [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Update))]
        public static bool Prefix(PlayerRoleManager __instance)
            => false;
    }
}