using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Other;

using PlayerStatsSystem;

namespace LabExtended.Patches.Events
{
    public static class PlayerAppliedDamagePatch
    {
        [HookPatch(typeof(PlayerApplyingDamageEventArgs))]
        [HarmonyPatch(typeof(StandardDamageHandler), nameof(StandardDamageHandler.ApplyDamage))]
        [HarmonyPrefix]
        public static bool Prefix(ReferenceHub ply, StandardDamageHandler __instance, ref DamageHandlerBase.HandlerOutput __result)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return false;

            var info = new DamageInfo(__instance);
            var args = new PlayerApplyingDamageEventArgs(info, player);

            if (!HookRunner.RunEvent(args, true))
            {
                __result = DamageHandlerBase.HandlerOutput.Nothing;
                return false;
            }

            return true;
        }

        [HookPatch(typeof(PlayerAppliedDamageEventArgs))]
        [HarmonyPatch(typeof(StandardDamageHandler), nameof(StandardDamageHandler.ApplyDamage))]
        [HarmonyPostfix]
        public static void Postfix(ReferenceHub ply, StandardDamageHandler __instance, DamageHandlerBase.HandlerOutput __result)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return;

            HookRunner.RunEvent(new PlayerAppliedDamageEventArgs(new(__instance), __result, player));
        }
    }
}