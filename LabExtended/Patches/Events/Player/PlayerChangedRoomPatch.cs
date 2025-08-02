using HarmonyLib;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Player;

using MapGeneration;

using PlayerRoles;

using RoundRestarting;

using UnityEngine;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="PlayerChangedRoomEventArgs"/> and <see cref="PlayerChangedZoneEventArgs"/> events.
/// </summary>
public static class PlayerChangedRoomPatch
{
    [HarmonyPatch(typeof(CurrentRoomPlayerCache), nameof(CurrentRoomPlayerCache.ValidateCache))]
    private static bool Prefix(CurrentRoomPlayerCache __instance)
    {
        if (RoundRestart.IsRoundRestarting)
            return false;

        if (Time.frameCount == __instance._lastFrame)
            return false;

        var position = __instance._tr.position;

        if (__instance._roleManager.CurrentRole is ICameraController cameraController)
            position = cameraController.CameraPosition;

        var room = position.TryGetRoom(out var r) ? r : null;

        if (room != null)
        {
            ExPlayer? targetPlayer = null;
            
            if ((__instance._lastDetected is null || __instance._lastDetected != room) && ExPlayer.TryGet(__instance._roleManager.Hub, out targetPlayer))
                ExPlayerEvents.OnChangedRoom(new(targetPlayer, room, __instance._lastDetected));

            if ((__instance._lastDetected?.Zone ?? FacilityZone.None) != room.Zone && (targetPlayer != null && ExPlayer.TryGet(__instance._roleManager.Hub, out targetPlayer)))
                ExPlayerEvents.OnChangedZone(new(targetPlayer, (__instance._lastDetected?.Zone ?? FacilityZone.None), room.Zone));
            
            __instance._lastDetected = room;
            __instance._lastValid = true;
            
            __instance._hasAnyValidRoom = true;
        }
        else
        {
            if (__instance._lastDetected != null && __instance._lastValid && ExPlayer.TryGet(__instance._roleManager.Hub, out var player))
            {
                ExPlayerEvents.OnChangedRoom(new(player, room, __instance._lastDetected));
                ExPlayerEvents.OnChangedZone(new(player, null, __instance._lastDetected.Zone));
            }

            __instance._lastValid = false;
        }

        __instance._lastFrame = Time.frameCount;
        return false;
    }
}