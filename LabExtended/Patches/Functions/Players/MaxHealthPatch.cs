using HarmonyLib;

using LabExtended.API;

using PlayerStatsSystem;

namespace LabExtended.Patches.Functions.Players
{
    public static class MaxHealthPatch
    {
        [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.MaxValue), MethodType.Getter)]
        public static bool Prefix(HealthStat __instance, ref float __result)
        {
            if (!ExPlayer.TryGet(__instance.Hub, out var player) || !player.Stats._healthOverride.HasValue)
                return true;

            __result = player.Stats._healthOverride.Value;
            return false;
        }
    }
}