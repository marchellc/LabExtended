using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events.Scp049;
using LabExtended.Extensions;

using Mirror;

using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Ragdolls;

using System.Reflection;

namespace LabExtended.Patches.Events
{
    public static class CancellingRessurectionPatch
    {
        public static readonly MethodInfo ReplacementMethod = typeof(CancellingRessurectionPatch).FindMethod("Prefix");
        public static readonly MethodInfo TargetMethod = typeof(RagdollAbilityBase<>).MakeGenericType(typeof(Scp049Role)).FindMethod("ServerProcessCmd");

        public static Harmony Harmony => ExLoader.Harmony;

        [OnLoad]
        public static void Enable()
            => Harmony.Patch(TargetMethod, new HarmonyMethod(ReplacementMethod));

        private static bool Prefix(RagdollAbilityBase<Scp049Role> __instance, NetworkReader reader)
        {
            if (__instance is null || __instance is not Scp049ResurrectAbility)
                return true;

            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            var pos = __instance.CastRole.FpcModule.Position;
            var cur = __instance.CurRagdoll;

            var ragdoll = reader.ReadNetworkBehaviour<DynamicRagdoll>();

            __instance._syncRagdoll = ragdoll;

            if (ragdoll is null)
            {
                if (!__instance.IsInProgress)
                    return false;

                __instance._errorCode = __instance.ServerValidateCancel();

                var cancellingArgs = new Scp049CancellingResurrectionArgs(scp, ExPlayer.Get(ragdoll?.Info.OwnerHub), __instance._errorCode);

                if (!HookRunner.RunCancellable(cancellingArgs, true) || cancellingArgs.ErrorCode != 0)
                {
                    __instance.ServerSendRpc(true);
                    return false;
                }

                __instance.IsInProgress = false;
                return false;
            }
            else
            {
                if (__instance.IsInProgress)
                    return false;

                if (!__instance.IsCorpseNearby(pos, ragdoll, out var transform))
                    return false;

                var curTransform = __instance._ragdollTransform;
                var curRagdoll = __instance.CurRagdoll;

                __instance._ragdollTransform = transform;
                __instance._syncRagdoll = ragdoll;
                __instance.CurRagdoll = ragdoll;
                __instance._errorCode = __instance.ServerValidateBegin(ragdoll);

                if (__instance._errorCode > 0 || !__instance.ServerValidateAny())
                {
                    __instance._ragdollTransform = curTransform;
                    __instance.CurRagdoll = curRagdoll;

                    if (__instance._errorCode > 0)
                        __instance.ServerSendRpc(true);

                    return false;
                }

                __instance.IsInProgress = true;
                return false;
            }
        }
    }
}
