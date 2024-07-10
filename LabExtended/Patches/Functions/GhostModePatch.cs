using Common.Extensions;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Collections.Locked;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Visibility;

using RelativePositioning;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class GhostModePatch
    {
        public static LockedHashSet<uint> GhostedPlayers { get; } = new LockedHashSet<uint>();
        public static LockedDictionary<uint, LockedList<uint>> GhostedTo { get; } = new LockedDictionary<uint, LockedList<uint>>();

        public static bool IsInvisible(uint receiverId, uint otherId)
        {
            if (GhostedPlayers.Contains(otherId))
                return true;

            if (GhostedTo.TryGetValue(otherId, out var ghostList) && ghostList.Contains(receiverId))
                return true;

            return false;
        }

        public static bool Prefix(ReferenceHub receiver, NetworkWriter writer)
        {
            var player = ExPlayer.Get(receiver);

            if (player is null)
                return true;

            var index = 0;
            var hasCustomVisibility = player.Role.Role.TryTypeCast<ICustomVisibilityRole>(out var customVisibilityRole);

            foreach (var other in ExPlayer.Players)
            {
                if (other.NetId == player.NetId)
                    continue;

                var fpcRole = other.Role.FpcRole;

                if (fpcRole is null)
                    continue;

                var isInvisible = hasCustomVisibility && !customVisibilityRole.VisibilityController.ValidateVisibility(other.Hub);

                if (IsInvisible(receiver.netId, other.NetId))
                    isInvisible = true;

                var syncData = GetNewSyncData(player, other, fpcRole.FpcModule, isInvisible);

                if (!isInvisible)
                {
                    FpcServerPositionDistributor._bufferPlayerIDs[index] = other.PlayerId;
                    FpcServerPositionDistributor._bufferSyncData[index] = syncData;

                    index++;
                }
            }

            writer.WriteUShort((ushort)index);

            for (int i = 0; i < index; i++)
            {
                writer.WriteRecyclablePlayerId(new RecyclablePlayerId(FpcServerPositionDistributor._bufferPlayerIDs[i]));
                FpcServerPositionDistributor._bufferSyncData[i].Write(writer);
            }

            return false;
        }

        private static FpcSyncData GetNewSyncData(ExPlayer receiver, ExPlayer target, FirstPersonMovementModule fpmm, bool isInvisible)
        {
            var prevSyncData = FpcServerPositionDistributor.GetPrevSyncData(receiver.Hub, target.Hub);
            var fpcSyncData = (isInvisible ? default(FpcSyncData) : new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, new RelativePosition(target.FakePosition.GetValue(receiver, target.Transform.position)), fpmm.MouseLook));

            FpcServerPositionDistributor.PreviouslySent[receiver.NetId][target.NetId] = fpcSyncData;
            return fpcSyncData;
        }
    }
}