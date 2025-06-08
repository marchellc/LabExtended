using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Scp939;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Patches.Events.Scp939
{
    public static class Scp939PlacingMimicryPatch
    {
        [HarmonyPatch(typeof(MimicPointController), nameof(MimicPointController.ServerProcessCmd))]
        public static bool Prefix(MimicPointController __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Toggles.CanUseMimicryAs939)
                return false;

            if (__instance.Active)
            {
                if (!ExScp939Events.OnRemovingMimicry(new(scp, __instance._syncPos, __instance._syncMessage)))
                    return false;

                __instance._syncMessage = MimicPointController.RpcStateMsg.RemovedByUser;
                __instance.Active = false;
            }
            else
            {
                var placingArgs = new Scp939PlacingMimicryEventArgs(scp, new RelativePosition(__instance.CastRole.FpcModule.Position));

                if (!ExScp939Events.OnPlacingMimicry(placingArgs))
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