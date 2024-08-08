using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Collections.Locked;

using RelativePositioning;

namespace LabExtended.Patches.Fixes
{
    [HarmonyPatch(typeof(NetIdWaypoint), nameof(NetIdWaypoint.SqrDistanceTo))]
    public static class NetIdWaypointIgnoreDoorsPatch
    {
        public static readonly LockedHashSet<NetIdWaypoint> DisabledWaypoints = new LockedHashSet<NetIdWaypoint>();

        public static bool Prefix(NetIdWaypoint __instance, ref float __result)
        {
            if (DisabledWaypoints.Contains(__instance)
                || (__instance._targetNetId != null && Prefabs._spawnedDoors.Any(x => x && x.netId == __instance._targetNetId.netId)))
            {
                __result = float.MaxValue;
                return false;
            }

            return true;
        }
    }
}