using HarmonyLib;

using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using UnityEngine;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerOverridingPositionPatch
    {
        [HarmonyPatch(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.ServerOverridePosition))]
        public static bool Prefix(FirstPersonMovementModule __instance, Vector3 position)
        {
            if (!ExPlayer.TryGet(__instance.Hub, out var player))
                return false;

            var overridingArgs = new PlayerOverridingPositionEventArgs(player, __instance.Position, position);

            if (!ExPlayerEvents.OnOverridingPosition(overridingArgs))
                return false;

            __instance.Position = overridingArgs.NewPosition;
            __instance.Hub.connectionToClient.Send(new FpcPositionOverrideMessage(overridingArgs.NewPosition));
            __instance.OnServerPositionOverwritten.InvokeSafe();

            return false;
        }
    }
}