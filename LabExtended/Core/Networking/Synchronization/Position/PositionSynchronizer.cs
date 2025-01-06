using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core.Ticking;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events.Player;
using LabExtended.Patches.Functions.Players;

using Mirror;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.Core.Networking.Synchronization.Position
{
    public static class PositionSynchronizer
    {
        private static NetworkWriter _writer = NetworkWriterPool.Get();

        private static float _sendTime = 0f;

        private static bool _restarting;
        private static bool _sending;
        private static bool _debug;

        internal static readonly List<ExPlayer> _validBuffer = ListPool<ExPlayer>.Shared.Rent();
        internal static readonly Dictionary<ExPlayer, Dictionary<ExPlayer, PositionData>> _syncCache = DictionaryPool<ExPlayer, Dictionary<ExPlayer, PositionData>>.Shared.Rent();

        public static bool ForceSendNextFrame { get; set; }

        public static bool IsSending => _sending;

        public static float SendRate => ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate > 0f ? ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate : FpcServerPositionDistributor.SendRate;

        private static void Synchronize()
        {
            if (_sending || ExPlayer._players.Count < 1)
                return;

            _sending = true;

            try
            {
                for (int i = 0; i < ExPlayer._allPlayers.Count; i++)
                {
                    var player = ExPlayer._allPlayers[i];

                    if (!player.Switches.ShouldReceivePositions)
                        continue;

                    var ghosted = ExPlayer._ghostedPlayers.Contains(player);
                    var hasRole = player.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole);

                    _validBuffer.Clear();

                    for (int x = 0; x < ExPlayer._allPlayers.Count; x++)
                    {
                        var other = ExPlayer._allPlayers[x];

                        if (!other.Switches.ShouldSendPosition)
                            continue;

                        if (!other.Role.Is<IFpcRole>(out _))
                            continue;

                        if (other.NetId == player.NetId && !player.Switches.ShouldReceiveOwnPosition)
                            continue;

                        _validBuffer.Add(other);
                    }

                    if (_validBuffer.Count < 1)
                        continue;

                    if (!_syncCache.TryGetValue(player, out var syncCache))
                        _syncCache[player] = syncCache = DictionaryPool<ExPlayer, PositionData>.Shared.Rent();

                    _writer.Reset();
                    _writer.WriteMessage<FpcPositionMessage>(_ =>
                    {
                        _writer.WriteUShort((ushort)_validBuffer.Count);

                        for (int y = 0; y < _validBuffer.Count; y++)
                        {
                            var other = _validBuffer[y];
                            var fpcRole = (IFpcRole)other.Role.Role;
                            var mouseLook = fpcRole.FpcModule.MouseLook;
                            var module = fpcRole.FpcModule;

                            var isInvisible = hasRole && customVisibilityRole.VisibilityController.ValidateVisibility(other.Hub);

                            if (!isInvisible && (ghosted || other._invisibility.Contains(player)))
                                isInvisible = true;

                            if (!syncCache.TryGetValue(other, out var prevData))
                                prevData = syncCache[other] = new PositionData();

                            var position = other.Transform.position;
                            var posBit = false;

                            if (player.Position.FakedList.GlobalValue != Vector3.zero)
                            {
                                position = player.Position.FakedList.GlobalValue;
                                posBit = true;
                            }

                            if (player.Position.FakedList.TryGetValue(other, out var fakedPos))
                            {
                                position = fakedPos;
                                posBit = true;
                            }

                            var relative = new RelativePosition(other.Hub);

                            if (!posBit)
                                posBit = prevData.Position != relative;

                            mouseLook.GetSyncValues(relative.WaypointId, out var syncH, out var syncV);

                            var lookBit = prevData.SyncH != syncH || prevData.SyncV != syncV;
                            var customBit = module.IsGrounded;

                            prevData.Position = relative;

                            prevData.SyncH = syncH;
                            prevData.SyncV = syncV;

                            Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3, out var b4, out var b5, out var b6, out var b7, out var b8);

                            _writer.WriteRecyclablePlayerId(other.Hub.Network_playerId);
                            _writer.WriteByte(Misc.BoolsToByte(b1, b1, b3, b4, b5, lookBit, posBit, customBit));

                            if (posBit)
                                _writer.WriteRelativePosition(relative);

                            if (lookBit)
                                _writer.WriteUShort(syncH);
                                _writer.WriteUShort(syncV);
                        }
                    });

                    player.Connection.Send(_writer.ToArraySegment());
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Position Synchronizer", $"Caught an error while synchronizing player positions:\n{ex.ToColoredString()}");
            }

            _sending = false;
        }

        private static void Update()
        {
            if (_sending)
                return;

            var sendRate = SendRate;

            _sendTime -= sendRate;

            if (_sendTime > 0f && !ForceSendNextFrame)
                return;

            _sendTime = sendRate;

            ForceSendNextFrame = false;

            Synchronize();
        }

        internal static void InternalHandleRoleChange(PlayerChangedRoleArgs args)
        {
            if (_restarting || args.PreviousRole is null || args.PreviousRole is not IFpcRole)
                return;

            _sending = true;
            _syncCache.ForEach(x => x.Value.Remove(args.Player));
            _sending = false;
        }

        internal static void InternalHandleLeave(ExPlayer player)
        {
            if (_restarting)
                return;

            _sending = true;

            if (_syncCache.TryGetValue(player, out var syncCache))
                DictionaryPool<ExPlayer, PositionData>.Shared.Return(syncCache);

            _syncCache.Remove(player);
            _syncCache.ForEach(x => x.Value.Remove(player));

            _sending = false;
        }

        internal static void InternalHandleRoundRestart()
        {
            _restarting = true;

            _validBuffer.Clear();
            _syncCache.Clear();
        }

        internal static void InternalHandleWaiting()
        {
            _restarting = false;
        }

        [LoaderInitialize(1)]
        internal static void InternalLoad()
        {
            PlayerLeavePatch.OnLeaving += InternalHandleLeave;
            ApiLoader.ApiConfig.TickSection.GetCustomOrDefault("PositionSync", TickDistribution.UnityTick).CreateHandle(TickDistribution.CreateWith(Update, new TickOptions(TickFlags.Separate)));
        }
    }
}