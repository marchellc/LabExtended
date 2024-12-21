using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    public static class PlayerTeleportPatch
    {
        [HookPatch(typeof(PlayerTeleportingArgs))]
        [HarmonyPatch(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.ServerOverridePosition))]
        public static bool Prefix(FirstPersonMovementModule __instance, Vector3 position)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            var teleportingArgs = new PlayerTeleportingArgs(player, __instance.Position, position);

            if (!HookRunner.RunEvent(teleportingArgs, true))
                return false;

            __instance.Position = teleportingArgs.NewPosition;
            __instance.Hub.connectionToClient.Send(new FpcPositionOverrideMessage(teleportingArgs.NewPosition));
            __instance.OnServerPositionOverwritten.InvokeSafe();

            return false;
        }
    }
}