using HarmonyLib;

using PlayerRoles.FirstPersonControl.NetworkMessages;

namespace LabExtended.Patches.Functions.Players
{
    public static class DisablePositionSyncPatch
    {
        [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.LateUpdate))]
        public static bool Prefix() => false;
    }
}