using HarmonyLib;

using LabExtended.API;

using MapGeneration;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;

using RelativePositioning;

namespace LabExtended.Patches.Functions.Scp106
{
    public static class Scp106PocketItemsPositionPatch
    {
        [HarmonyPatch(typeof(Scp106PocketItemManager), nameof(Scp106PocketItemManager.GetRandomValidSpawnPosition))]
        public static bool Prefix(ref RelativePosition __result)
        {
            var count = 0;

            foreach (var player in ExPlayer.AllPlayers)
            {
                if (!player.Switches.CanBePocketDimensionItemTarget)
                    continue;

                if (!player.Role.Is<IFpcRole>(out var fpcRole))
                    continue;

                var pos = fpcRole.FpcModule.Position;

                if (pos.y >= Scp106PocketItemManager.HeightLimit.x && Scp106PocketItemManager.TryGetRoofPosition(pos, out var roofPos))
                {
                    Scp106PocketItemManager.ValidPositionsNonAlloc[count] = roofPos;

                    if (++count > 64)
                        break;
                }
            }

            if (count > 0)
                __result = new RelativePosition(Scp106PocketItemManager.ValidPositionsNonAlloc[UnityEngine.Random.Range(0, count)]);
            else
            {
                foreach (var room in RoomIdentifier.AllRoomIdentifiers)
                {
                    if ((room.Zone == FacilityZone.HeavyContainment || room.Zone == FacilityZone.Entrance)
                        && Scp106PocketItemManager.TryGetRoofPosition(room.transform.position, out var roofPos))
                    {
                        Scp106PocketItemManager.ValidPositionsNonAlloc[count] = roofPos;

                        if (++count > 64)
                            break;
                    }
                }

                __result = new RelativePosition(Scp106PocketItemManager.ValidPositionsNonAlloc[UnityEngine.Random.Range(0, count)]);
            }

            return false;
        }
    }
}