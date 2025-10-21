using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Events;
using LabExtended.Events.Player;

using MapGeneration;

using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp173;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp173
{
    /// <summary>
    /// Implements functionality of the <see cref="SwitchContainer.CanBeBlockedAs173"/> and <see cref="SwitchContainer.CanBlockScp173"/>, as
    /// well as the <see cref="PlayerObservingScp173EventArgs"/> event.
    /// </summary>
    public static class Scp173BlockPatch
    {
        [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.IsObservedBy))]
        private static bool Prefix(Scp173ObserversTracker __instance, ReferenceHub target, float widthMultiplier, ref bool __result)
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
            var maxDistance = __instance._maxViewDistance;
            var roomIdentifier = position.TryGetRoom(out var room) ? room : null;

            if (room != null && room.Zone is FacilityZone.Surface)
                maxDistance *= 2f;

            var vision = VisionInformation.GetVisionInformation(target, target.PlayerCameraReference, position, __instance._modelWidth, maxDistance, 
                false, false, 0, true);

            if (!vision.IsLooking)
                return false;

            var position2 = target.PlayerCameraReference.position;
            var vector = target.PlayerCameraReference.TransformDirection(Vector3.right);
            var visibilityReferencePoints = __instance._visibilityReferencePoints;

            for (int i = 0; i < visibilityReferencePoints.Length; i++)
            {
                var vector2 = visibilityReferencePoints[i];
                var vector3 = position + vector2.x * widthMultiplier * vector + Vector3.up * vector2.y;

                if (!Physics.Linecast(vector3, position2, VisionInformation.VisionLayerMask)
                    && !Physics.Linecast(position, vector3, VisionInformation.VisionLayerMask))
                {
                    var lookingEv = new PlayerObservingScp173EventArgs(scp, player, __instance.CastRole, __instance, vision, vision.IsLooking);

                    if (!ExPlayerEvents.OnObservingScp173(lookingEv)
                        || !lookingEv.IsLooking)
                        return __result = false;

                    __result = true;
                    return false;
                }
            }

            return __result = false;
        }
    }
}