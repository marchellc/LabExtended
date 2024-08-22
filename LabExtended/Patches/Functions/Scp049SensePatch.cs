using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp049;
using LabExtended.Extensions;

using Mirror;

using PlayerRoles.FirstPersonControl;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp049;

using Utils.Networking;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp049SenseAbility), nameof(Scp049SenseAbility.ServerProcessCmd))]
    public static class Scp049SensePatch
    {
        public static double NullTargetCooldown = 2.5;

        public static bool Prefix(Scp049SenseAbility __instance, NetworkReader reader)
        {
            if (!__instance.Cooldown.IsReady || !__instance.Duration.IsReady)
                return false;

            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            __instance.HasTarget = false;
            __instance.Target = reader.ReadReferenceHub();

            var target = ExPlayer.Get(__instance.Target);

            if (target is null || !target.Switches.CanBeScp049Target || !scp.Switches.CanUseSenseAs049)
            {
                if (NullTargetCooldown > 0)
                    __instance.Cooldown.Trigger(NullTargetCooldown);

                __instance.ServerSendRpc(true);
                return false;
            }

            if (!HitboxIdentity.IsEnemy(__instance.Owner, __instance.Target))
                return false;

            if (!__instance.Target.roleManager.CurrentRole.Is<FpcStandardRoleBase>(out var fpc))
                return false;

            if (!VisionInformation.GetVisionInformation(__instance.Owner, __instance.Owner.PlayerCameraReference, fpc.CameraPosition, fpc.FpcModule.CharController.radius, __instance._distanceThreshold, true, true, 0, false).IsLooking)
                return false;

            var sensingArgs = new Scp049SensingTargetArgs(scp, target, __instance.CastRole, __instance);

            if (!HookRunner.RunEvent(sensingArgs, true) || sensingArgs.Target is null)
            {
                if (sensingArgs.Cooldown > 0)
                    __instance.Cooldown.Trigger(sensingArgs.Cooldown);

                __instance.ServerSendRpc(true);
                return false;
            }

            if (sensingArgs.Duration > 0)
                __instance.Duration.Trigger(sensingArgs.Duration);

            __instance.Target = sensingArgs.Target.Hub;
            __instance.HasTarget = true;

            __instance.ServerSendRpc(true);
            return false;
        }
    }
}