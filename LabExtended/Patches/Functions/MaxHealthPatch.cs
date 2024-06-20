using HarmonyLib;

using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.MaxValue), MethodType.Getter)]
    public static class MaxHealthPatch
    {
        public static bool Prefix(HealthStat __instance, ref float __result)
        {
            if (__instance.Hub is null)
                return true;

            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            if (!player.Stats._maxHealthOverride.IsActive)
                return true;

            __result = player.Stats._maxHealthOverride.Value;
            return false;
        }
    }
}