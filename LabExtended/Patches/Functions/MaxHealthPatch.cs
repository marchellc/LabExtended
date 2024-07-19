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
            if (!ExPlayer.TryGet(__instance.Hub, out var player) || !player.Stats._maxHealthOverride.HasValue)
                return true;

            __result = player.Stats._maxHealthOverride.Value;
            return false;
        }
    }
}