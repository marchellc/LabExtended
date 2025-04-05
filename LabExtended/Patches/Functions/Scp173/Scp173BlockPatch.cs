using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Events.Player;

using MapGeneration;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp173;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp173
{
    public static class Scp173BlockPatch
    {
        [EventPatch(typeof(PlayerObservingScp173EventArgs), true)]
        [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.IsObservedBy))]
        public static bool Prefix(Scp173ObserversTracker __instance, ReferenceHub target, float widthMultiplier, ref bool __result)
        {
            var scp = ExPlayer.Get(__instance.Owner);
            var player = ExPlayer.Get(target);

            if (scp is null || player is null)
                return true;

            if (!scp.Toggles.CanBeBlockedAs173)
                return __result = false;

            if (!player.Toggles.CanBlockScp173)
                return __result = false;

            var position = __instance.CastRole.FpcModule.Position;
            var roomIdentifier = RoomUtils.RoomAtPosition(position);
            var vision = VisionInformation.GetVisionInformation(target, target.PlayerCameraReference, position, __instance._modelWidth, roomIdentifier != null && roomIdentifier.Zone == FacilityZone.Surface ? __instance._maxViewDistance * 2f : __instance._maxViewDistance, checkFog: false, checkLineOfSight: false);

            if (!vision.IsLooking)
                return false;

            var position2 = target.PlayerCameraReference.position;
            var vector = target.PlayerCameraReference.TransformDirection(Vector3.right);
            var visibilityReferencePoints = __instance._visibilityReferencePoints;

            for (int i = 0; i < visibilityReferencePoints.Length; i++)
            {
                var vector2 = visibilityReferencePoints[i];

                if (!Physics.Linecast(position + vector2.x * widthMultiplier * vector + Vector3.up * vector2.y, position2, 
                        VisionInformation.VisionLayerMask))
                {
                    var lookingEv = new PlayerObservingScp173EventArgs(scp, player, __instance.CastRole, __instance, vision, vision.IsLooking);

                    if (!ExPlayerEvents.OnObservingScp173(lookingEv))
                        return __result = false;

                    if (!lookingEv.IsLooking)
                        return __result = false;

                    __result = true;
                    return false;
                }
            }

            return __result = false;
        }
    }
}