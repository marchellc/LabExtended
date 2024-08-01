using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp939;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(MimicPointController), nameof(MimicPointController.ServerProcessCmd))]
    public static class Scp939PlacingMimicryPatch
    {
        public static bool Prefix(MimicPointController __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Switches.CanUseMimicryAs939)
                return false;

            if (__instance.Active)
            {
                if (!HookRunner.RunCancellable(new Scp939RemovingMimicryArgs(scp, __instance._syncPos, MimicPointController.RpcStateMsg.RemovedByUser), true))
                    return false;

                __instance._syncMessage = MimicPointController.RpcStateMsg.RemovedByUser;
                __instance.Active = false;
            }
            else
            {
                var placingArgs = new Scp939PlacingMimicryArgs(scp, new RelativePosition(__instance.CastRole.FpcModule.Position));

                if (!HookRunner.RunCancellable(placingArgs, true))
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