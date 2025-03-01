using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Other;

using PlayerStatsSystem;

namespace LabExtended.Patches.Events
{
    public static class AppliedDamagePatch
    {
        [HookPatch(typeof(ApplyingDamageArgs))]
        [HarmonyPatch(typeof(StandardDamageHandler), nameof(StandardDamageHandler.ApplyDamage))]
        [HarmonyPrefix]
        public static bool Prefix(ReferenceHub ply, StandardDamageHandler __instance, ref DamageHandlerBase.HandlerOutput __result)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return false;

            var info = new DamageInfo(__instance);
            var args = new ApplyingDamageArgs(info, player);

            if (!HookRunner.RunEvent(args, true))
            {
                __result = DamageHandlerBase.HandlerOutput.Nothing;
                return false;
            }

            return true;
        }

        [HookPatch(typeof(AppliedDamageArgs))]
        [HarmonyPatch(typeof(StandardDamageHandler), nameof(StandardDamageHandler.ApplyDamage))]
        [HarmonyPostfix]
        public static void Postfix(ReferenceHub ply, StandardDamageHandler __instance, DamageHandlerBase.HandlerOutput __result)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return;

            HookRunner.RunEvent(new AppliedDamageArgs(new(__instance), __result, player));
        }
    }
}