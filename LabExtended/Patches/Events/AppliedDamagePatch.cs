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
            var info = DamageInfo.Get(__instance);
            var player = ExPlayer.Get(ply);

            if (player is null)
                return true;

            if (info.IsApplied)
                return false;

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
        public static void Postfix(ReferenceHub ply, StandardDamageHandler __instance, ref DamageHandlerBase.HandlerOutput __result)
        {
            var info = DamageInfo.Get(__instance);
            var player = ExPlayer.Get(ply);

            if (player is null)
                return;

            if (info.IsApplied)
                return;

            var args = new AppliedDamageArgs(info, __result, player);

            HookRunner.RunEvent(args);

            __result = args.Output;

            if (__result != DamageHandlerBase.HandlerOutput.Nothing)
                info.IsApplied = true;
        }
    }
}