using HarmonyLib;

using LabExtended.API;

using PlayerRoles.PlayableScps.Scp096;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.IsObservedBy))]
    public static class Scp096TriggerPatch
    {
        public static bool Prefix(Scp096TargetsTracker __instance, ReferenceHub target, ref bool __result)
        {
            var scp = ExPlayer.Get(__instance.Owner);
            var player = ExPlayer.Get(target);

            if (scp is null || player is null)
                return true;

            if (!scp.Switches.CanBeTriggered)
            {
                __result = false;
                return false;
            }

            if (!player.Switches.CanTriggerScp096)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}