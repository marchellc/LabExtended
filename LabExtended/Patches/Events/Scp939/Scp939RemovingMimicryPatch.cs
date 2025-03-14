using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Scp939;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

namespace LabExtended.Patches.Events.Scp939
{
    public static class Scp939RemovingMimicryPatch
    {
        [EventPatch(typeof(Scp939RemovingMimicryEventArgs))]
        [HarmonyPatch(typeof(MimicPointController), nameof(MimicPointController.UpdateMimicPoint))]
        public static bool Prefix(MimicPointController __instance)
        {
            __instance.MimicPointTransform.position = __instance._syncPos.Position;

            if (__instance.Distance < __instance.MaxDistance)
                return false;

            if (ExPlayer.TryGet(__instance.Owner, out var player))
            {
                var removingArgs = new Scp939RemovingMimicryEventArgs(player, __instance._syncPos, MimicPointController.RpcStateMsg.DestroyedByDistance);

                if (!ExScp939Events.OnRemovingMimicry(removingArgs))
                    return false;
            }

            __instance._syncMessage = MimicPointController.RpcStateMsg.DestroyedByDistance;
            __instance.ServerSendRpc(true);

            return false;
        }
    }
}
