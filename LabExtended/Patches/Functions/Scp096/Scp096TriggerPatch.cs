using HarmonyLib;

using LabExtended.API;

using PlayerRoles.PlayableScps.Scp096;

namespace LabExtended.Patches.Functions.Scp096
{
    public static class Scp096TriggerPatch
    {
        [HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.IsObservedBy))]
        public static bool Prefix(Scp096TargetsTracker __instance, ReferenceHub target, ref bool __result)
        {
            var scp = ExPlayer.Get(__instance.Owner);
            var player = ExPlayer.Get(target);

            if (scp is null || player is null)
                return true;

            if (!scp.Toggles.CanBeTriggeredAs096)
            {
                __result = false;
                return false;
            }

            if (!player.Toggles.CanTriggerScp096)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}