using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp939;

using PlayerRoles.PlayableScps.Scp939.Mimicry;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(MimicPointController), nameof(MimicPointController.UpdateMimicPoint))]
    public static class Scp939RemovingMimicryPatch
    {
        public static bool Prefix(MimicPointController __instance)
        {
            __instance.MimicPointTransform.position = __instance._syncPos.Position;

            if (__instance.Distance < __instance.MaxDistance)
                return false;

            if (ExPlayer.TryGet(__instance.Owner, out var player))
            {
                var removingArgs = new Scp939RemovingMimicryArgs(player, __instance._syncPos, MimicPointController.RpcStateMsg.DestroyedByDistance);

                if (!HookRunner.RunEvent(removingArgs, true))
                    return false;
            }

            __instance._syncMessage = MimicPointController.RpcStateMsg.DestroyedByDistance;
            __instance.ServerSendRpc(true);

            return false;
        }
    }
}
