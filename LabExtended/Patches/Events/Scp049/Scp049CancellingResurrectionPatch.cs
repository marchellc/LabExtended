using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Scp049;

using Mirror;

using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Ragdolls;

using System.Reflection;

namespace LabExtended.Patches.Events.Scp049
{
    public static class Scp049CancellingResurrectionPatch
    {
        public static readonly MethodInfo ReplacementMethod = typeof(Scp049CancellingResurrectionPatch).FindMethod("Prefix");
        public static readonly MethodInfo TargetMethod = typeof(RagdollAbilityBase<>).MakeGenericType(typeof(Scp049Role))
            .FindMethod("ServerProcessCmd");

        public static MethodInfo? PatchMethod;

        public static Harmony Harmony => ApiPatcher.Harmony;

        [LoaderInitialize(1)]
        public static void Enable()
            => PatchMethod = Harmony.Patch(TargetMethod, new HarmonyMethod(ReplacementMethod));

        private static bool Prefix(RagdollAbilityBase<Scp049Role> __instance, NetworkReader reader)
        {
            if (__instance is not Scp049ResurrectAbility)
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

                var cancellingArgs = new Scp049CancellingResurrectionEventArgs(scp, ExPlayer.Get(ragdoll?.Info.OwnerHub), __instance._errorCode);

                if (!ExScp049Events.OnCancellingResurrection(cancellingArgs) || cancellingArgs.ErrorCode != 0)
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
