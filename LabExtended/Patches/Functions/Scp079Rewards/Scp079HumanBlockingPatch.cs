using HarmonyLib;

using LabExtended.API;

using MapGeneration;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp079.Rewards;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp079Rewards
{
    public static class Scp079HumanBlockingPatch
    {
        [HarmonyPatch(typeof(HumanBlockingRewards), nameof(HumanBlockingRewards.CheckRoom))]
        public static bool Prefix(RoomIdentifier room, Vector3 doorPos, ref bool __result)
        {
            HumanBlockingRewards.RoomScps.Clear();
            HumanBlockingRewards.RoomHumans.Clear();

            var anyScps = false;
            var anyHumans = false;

            foreach (var player in ExPlayer.Players)
            {
                if (!player.Toggles.CanCountAs079ExpTarget)
                    continue;

                if (!player.Role.Is<IFpcRole>(out var fpcRole))
                    continue;

                var plyRoom = fpcRole.FpcModule.Position.TryGetRoom(out var pRoom) ? pRoom : null;

                if (plyRoom is null || plyRoom != room)
                    continue;

                if (player.Role.IsScp)
                {
                    HumanBlockingRewards.RoomScps.Add(fpcRole.FpcModule);
                    anyScps = true;
                }
                else
                {
                    HumanBlockingRewards.RoomHumans.Add(fpcRole.FpcModule);
                    anyHumans = true;
                }
            }

            if (!anyHumans || !anyScps)
                return __result = false;

            foreach (var module in HumanBlockingRewards.RoomScps)
            {
                var direction = HumanBlockingRewards.NormalizeIgnoreY(module.Motor.MoveDirection.normalized);
                var position = HumanBlockingRewards.NormalizeIgnoreY(doorPos - module.Position);

                if (Vector3.Dot(direction, position) >= 0.5f)
                {
                    foreach (var humanModule in HumanBlockingRewards.RoomHumans)
                    {
                        var humanDirection = humanModule.Position - module.Position;

                        if (humanDirection.sqrMagnitude <= 400f && Vector3.Dot(direction, HumanBlockingRewards.NormalizeIgnoreY(humanDirection)) >= 0.5f)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }

            __result = false;
            return false;
        }
    }
}