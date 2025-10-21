using HarmonyLib;

using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Extensions;

using Mirror;

using PlayerRoles.FirstPersonControl;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp049;

using Utils.Networking;

namespace LabExtended.Patches.Functions.Scp049
{
    /// <summary>
    /// Implements the functionality of the <see cref="SwitchContainer.CanBeScp049Target"/> and <see cref="SwitchContainer.CanUseSenseAs049"/> toggles.
    /// </summary>
    public static class Scp049SensePatch
    {
        [HarmonyPatch(typeof(Scp049SenseAbility), nameof(Scp049SenseAbility.ServerProcessCmd))]
        private static bool Prefix(Scp049SenseAbility __instance, NetworkReader reader)
        {
            if (!__instance.Cooldown.IsReady || !__instance.Duration.IsReady)
                return false;

            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            __instance.HasTarget = false;
            __instance.Target = reader.ReadReferenceHub();

            var target = ExPlayer.Get(__instance.Target);

            if (target is null || !target.Toggles.CanBeScp049Target || !scp.Toggles.CanUseSenseAs049)
            {
                if (Scp049SenseAbility.AttemptFailCooldown > 0)
                    __instance.Cooldown.Trigger(Scp049SenseAbility.AttemptFailCooldown);

                __instance.ServerSendRpc(true);
                return false;
            }

            if (!HitboxIdentity.IsEnemy(__instance.Owner, __instance.Target))
                return false;

            if (!__instance.Target.roleManager.CurrentRole.Is<FpcStandardRoleBase>(out var fpc))
                return false;

            if (!VisionInformation.GetVisionInformation(__instance.Owner, __instance.Owner.PlayerCameraReference, 
                    fpc.CameraPosition, fpc.FpcModule.CharController.radius, __instance._distanceThreshold, 
                    true, true, 0, false).IsLooking)
                return false;

            var sensingArgs = new Scp049UsingSenseEventArgs(scp.ReferenceHub, target.ReferenceHub);

            Scp049Events.OnUsingSense(sensingArgs);

            if (!sensingArgs.IsAllowed)
            {
                __instance.ServerSendRpc(true);
                return false;
            }

            __instance.Duration.Trigger(Scp049SenseAbility.EffectDuration);

            __instance.Target = sensingArgs.Target.ReferenceHub;
            __instance.HasTarget = true;

            __instance.ServerSendRpc(true);
            return false;
        }
    }
}