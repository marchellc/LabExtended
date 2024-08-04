using HarmonyLib;

using PlayerRoles.FirstPersonControl.NetworkMessages;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.LateUpdate))]
    public static class DisablePositionSyncPatch
    {
        public static bool Prefix() => false;
    }
}