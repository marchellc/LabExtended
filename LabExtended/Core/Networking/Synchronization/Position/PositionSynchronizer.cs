using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Patches.Functions.Players;
using LabExtended.Utilities.Unity;

using Mirror;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using RelativePositioning;

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LabExtended.Core.Networking.Synchronization.Position
{
    public static class PositionSynchronizer
    {
        private struct PositionUpdateLoop { }
        
        private static NetworkWriter _writer = NetworkWriterPool.Get();

        private static float _sendTime = 0f;

        internal static readonly List<ExPlayer> _validBuffer = ListPool<ExPlayer>.Shared.Rent();
        internal static readonly Dictionary<ExPlayer, Dictionary<ExPlayer, PositionData>> _syncCache = DictionaryPool<ExPlayer, Dictionary<ExPlayer, PositionData>>.Shared.Rent();

        public static float SendRate => ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate > 0f 
            ? ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate 
            : FpcServerPositionDistributor.SendRate;

        private static void OnUpdate()
        {
            var sendRate = SendRate;

            _sendTime -= sendRate;

            if (_sendTime > 0f)
                return;

            _sendTime = sendRate;
            
            try
            {
                for (int i = 0; i < ExPlayer.AllPlayers.Count; i++)
                {
                    var player = ExPlayer.AllPlayers[i];

                    if (!player.Switches.ShouldReceivePositions)
                        continue;

                    var ghosted = ExPlayer.ghostedPlayers.Contains(player);
                    var hasRole = player.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole);

                    _validBuffer.Clear();

                    for (int x = 0; x < ExPlayer.AllPlayers.Count; x++)
                    {
                        var other = ExPlayer.AllPlayers[x];

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

                            if (!isInvisible && (ghosted || other.invisibility.Contains(player)))
                                isInvisible = true;

                            if (!syncCache.TryGetValue(other, out var prevData))
                                prevData = syncCache[other] = new PositionData();

                            var position = isInvisible ? FpcMotor.InvisiblePosition : other.Transform.position;
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

                            var relative = new RelativePosition(position);

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
                            {
                                _writer.WriteUShort(syncH);
                                _writer.WriteUShort(syncV);
                            }
                        }
                    });

                    player.Send(_writer.ToArraySegment());
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Position Synchronizer", $"Caught an error while synchronizing player positions:\n{ex.ToColoredString()}");
            }
        }

        private static void OnPlayerRoleChange(PlayerChangedRoleArgs args)
        {
            if (args.PreviousRole is null || args.PreviousRole is not IFpcRole)
                return;
            
            _syncCache.ForEach(x => x.Value.Remove(args.Player));
        }

        private static void OnSpawning(PlayerSpawningArgs args)
        {
            _syncCache.Remove(args.Player);

            foreach (var player in ExPlayer.AllPlayers)
            {
                if (_syncCache.TryGetValue(player, out var cache))
                    cache.Remove(args.Player);
            }
        }

        private static void OnPlayerLeave(ExPlayer player)
        {
            if (_syncCache.TryGetValue(player, out var syncCache))
                DictionaryPool<ExPlayer, PositionData>.Shared.Return(syncCache);

            _syncCache.Remove(player);
            _syncCache.ForEach(x => x.Value.Remove(player));
        }

        private static void OnRoundRestart()
        {
            _validBuffer.Clear();
            _syncCache.Clear();
        }

        [LoaderInitialize(3)]
        private static void Init()
        {
            PlayerLeavePatch.OnLeaving += OnPlayerLeave;

            InternalEvents.OnRoleChanged += OnPlayerRoleChange;
            InternalEvents.OnRoundRestart += OnRoundRestart;
            InternalEvents.OnSpawning += OnSpawning;

            PlayerLoopHelper.ModifySystem(x =>
            {
                if (!x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(PositionUpdateLoop)))
                    return null;

                return x;
            });
        }
    }
}