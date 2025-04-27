using HarmonyLib;

using LabExtended.API;
using LabExtended.Events;

using MapGeneration;

using PlayerRoles;

using RoundRestarting;

using UnityEngine;

namespace LabExtended.Patches.Events.Player;

public static class PlayerChangedRoomPatch
{
    [HarmonyPatch(typeof(CurrentRoomPlayerCache), nameof(CurrentRoomPlayerCache.ValidateCache))]
    public static bool Prefix(CurrentRoomPlayerCache __instance)
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
            if (__instance._lastDetected is null || __instance._lastDetected != room)
                ExPlayerEvents.OnChangedRoom(new(ExPlayer.Get(__instance._roleManager.Hub), room,
                    __instance._lastDetected));

            if ((__instance._lastDetected?.Zone ?? FacilityZone.None) != room.Zone)
                ExPlayerEvents.OnChangedZone(new(ExPlayer.Get(__instance._roleManager.Hub),
                    (__instance._lastDetected?.Zone ?? FacilityZone.None), room.Zone));
            
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