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
        public struct PositionUpdateLoop { }
        
        private static NetworkWriter _writer = NetworkWriterPool.Get();
        private static float _sendTime = 0f;

        internal static readonly List<ExPlayer> _validBuffer = ListPool<ExPlayer>.Shared.Rent();

        public static float SendRate => ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate > 0f 
            ? ApiLoader.ApiConfig.SynchronizationSection.PositionSyncRate 
            : FpcServerPositionDistributor.SendRate;

        private static void OnUpdate()
        {
            _sendTime -= Time.deltaTime;

            if (_sendTime > 0f)
                return;

            _sendTime = SendRate;

            try
            {
                ExPlayer.AllPlayers.ForEach(player =>
                {
                    if (player is null || !player.Switches.ShouldReceivePositions)
                        return;

                    var ghosted = (ExPlayer.GhostedFlags & (1 << player.PlayerId)) != 0;
                    var hasRole = player.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole);

                    _validBuffer.Clear();

                    ExPlayer.AllPlayers.ForEach(other =>
                    {
                        if (!other.Switches.ShouldSendPosition) return;
                        if (!other.Role.Is<IFpcRole>(out _)) return;
                        if (other.NetId == player.NetId && !player.Switches.ShouldReceiveOwnPosition) return;

                        _validBuffer.Add(other);
                    });


                    if (_validBuffer.Count < 1)
                        return;

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

                            var isInvisible = hasRole &&
                                              !customVisibilityRole.VisibilityController.ValidateVisibility(other.Hub);

                            if (!isInvisible && (ghosted || (other.PersonalGhostFlags & (1 << player.PlayerId)) != 0))
                                isInvisible = true;

                            if (!player.sentPositions.TryGetValue(other.NetId, out var prevData))
                                player.sentPositions[other.NetId] = prevData = new();

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

                            if (prevData.IsReset || prevData.Position != position)
                                posBit = true;

                            mouseLook.GetSyncValues(relative.WaypointId, out var syncH, out var syncV);

                            var lookBit = prevData.IsReset || prevData.SyncH != syncH || prevData.SyncV != syncV;
                            var customBit = module.IsGrounded;

                            prevData.Position = position;
                            prevData.RelativePosition = relative;

                            prevData.SyncH = syncH;
                            prevData.SyncV = syncV;

                            Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3,
                                out var b4, out var b5, out var b6, out var b7, out var b8);

                            _writer.WriteRecyclablePlayerId(other.Hub.Network_playerId);
                            _writer.WriteByte(Misc.BoolsToByte(b1, b1, b3, b4, b5, lookBit, posBit, customBit));

                            if (posBit)
                                _writer.WriteRelativePosition(relative);

                            if (lookBit)
                            {
                                _writer.WriteUShort(syncH);
                                _writer.WriteUShort(syncV);
                            }

                            prevData.IsReset = false;
                        }
                    });

                    player.Send(_writer.ToArraySegment());
                });
            }
            catch (Exception ex)
            {
                ApiLog.Error("Position Synchronizer",
                    $"Caught an error while synchronizing player positions:\n{ex.ToColoredString()}");
            }
        }

        private static void OnPlayerRoleChange(PlayerChangedRoleArgs args)
        {
            if (args.PreviousRole is not IFpcRole || args.Player is null)
                return;
            
            ExPlayer.AllPlayers.ForEach(ply =>
            {
                if (ply != args.Player && args.Player.sentPositions.TryGetValue(ply.NetId, out var sent))
                {
                    sent.IsReset = true;
                }
            });
        }
        
        [LoaderInitialize(3)]
        private static void Init()
        {
            InternalEvents.OnRoleChanged += OnPlayerRoleChange;
            InternalEvents.OnRoundRestart += _validBuffer.Clear;

            PlayerLoopHelper.ModifySystem(x =>
            {
                if (!x.InjectAfter<TimeUpdate.WaitForLastPresentationAndUpdateTime>(OnUpdate, typeof(PositionUpdateLoop)))
                    return null;

                return x;
            });
        }
    }
}