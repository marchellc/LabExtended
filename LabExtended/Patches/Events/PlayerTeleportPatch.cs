using Common.Extensions;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.ServerOverridePosition))]
    public static class PlayerTeleportPatch
    {
        public static bool Prefix(FirstPersonMovementModule __instance, Vector3 position, Vector3 deltaRotation)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            var teleportingArgs = new PlayerTeleportingArgs(player, __instance.Position, position, deltaRotation);

            if (!HookRunner.RunCancellable(teleportingArgs, true))
                return false;

            __instance.Position = teleportingArgs.NewPosition;
            __instance.Hub.connectionToClient.Send(new FpcOverrideMessage(teleportingArgs.NewPosition, teleportingArgs.DeltaRotation.y));
            __instance.OnServerPositionOverwritten?.Call();

            HookRunner.RunEvent(new PlayerTeleportedArgs(player, teleportingArgs.CurrentPosition, teleportingArgs.NewPosition, teleportingArgs.DeltaRotation));
            return false;
        }
    }
}