using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp939;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Patches.Events
{
    public static class Scp939PlacingMimicryPatch
    {
        [HookPatch(typeof(Scp939PlacingMimicryArgs), true)]
        [HookPatch(typeof(Scp939RemovingMimicryArgs), true)]
        [HarmonyPatch(typeof(MimicPointController), nameof(MimicPointController.ServerProcessCmd))]
        public static bool Prefix(MimicPointController __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Switches.CanUseMimicryAs939)
                return false;

            if (__instance.Active)
            {
                if (!HookRunner.RunEvent(new Scp939RemovingMimicryArgs(scp, __instance._syncPos, MimicPointController.RpcStateMsg.RemovedByUser), true))
                    return false;

                __instance._syncMessage = MimicPointController.RpcStateMsg.RemovedByUser;
                __instance.Active = false;
            }
            else
            {
                var placingArgs = new Scp939PlacingMimicryArgs(scp, new RelativePosition(__instance.CastRole.FpcModule.Position));

                if (!HookRunner.RunEvent(placingArgs, true))
                    return false;

                __instance._syncMessage = MimicPointController.RpcStateMsg.PlacedByUser;
                __instance._syncPos = placingArgs.Position;

                __instance.Active = true;
            }

            __instance.ServerSendRpc(true);
            return false;
        }
    }
}