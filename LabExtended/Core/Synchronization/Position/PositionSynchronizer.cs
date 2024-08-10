using LabExtended.API;
using LabExtended.API.Pooling;

using LabExtended.Attributes;
using LabExtended.Core.Profiling;
using LabExtended.Core.Ticking;
using LabExtended.Events.Player;
using LabExtended.Extensions;
using LabExtended.Patches.Functions;
using LabExtended.Utilities;

using Mirror;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using RelativePositioning;

using UnityEngine;

namespace LabExtended.Core.Synchronization.Position
{
    public static class PositionSynchronizer
    {
        private static ProfilerMarker _marker = new ProfilerMarker("Position Synchronization", 1000);

        private static float _sendTime = 0f;
        private static float _debugTime = 0f;

        private static bool _restarting;
        private static bool _sending;
        private static bool _debug;

        private static readonly List<ExPlayer> _validBuffer = ListPool<ExPlayer>.Shared.Rent();
        private static readonly Dictionary<ExPlayer, Dictionary<ExPlayer, PositionData>> _syncCache = DictionaryPool<ExPlayer, Dictionary<ExPlayer, PositionData>>.Rent();

        public static bool ForceSendNextFrame { get; set; }

        public static bool IsSending => _sending;
        public static bool IsDebug => ApiLoader.Config.ApiOptions.PositionSynchronizerOptions.EnablePositionSyncDebug && _debug;

        public static float SendRate => ApiLoader.Config.ApiOptions.PositionSynchronizerOptions.PositionSyncRate > 0f ? ApiLoader.Config.ApiOptions.PositionSynchronizerOptions.PositionSyncRate : FpcServerPositionDistributor.SendRate;

        public static void Synchronize()
        {
            if (_sending || ExPlayer._players.Count < 1)
                return;

            _sending = true;
            _debug = _debugTime <= 0f;
            _marker.MarkStart();

            if (IsDebug)
                ApiLoader.Debug("Position Synchronizer", $"Synchronizing positions for {ExPlayer._allPlayers.Count} players.");

            try
            {
                for (int i = 0; i < ExPlayer._allPlayers.Count; i++)
                {
                    var player = ExPlayer._allPlayers[i];

                    if (IsDebug)
                        ApiLoader.Debug("Position Synchronizer", $"Synchronizing position for {player.Name} {player.UserId}");

                    if (!player.Switches.ShouldReceivePositions)
                    {
                        if (IsDebug)
                            ApiLoader.Debug("Position Synchronizer", $"ShouldReceivePositions=false");

                        continue;
                    }

                    var ghosted = ExPlayer._ghostedPlayers.Contains(player);
                    var hasRole = player.Role.Is<ICustomVisibilityRole>(out var customVisibilityRole);

                    _validBuffer.Clear();

                    for (int x = 0; x < ExPlayer._allPlayers.Count; x++)
                    {
                        var other = ExPlayer._allPlayers[x];

                        if (IsDebug)
                            ApiLoader.Debug("Position Synchronizer", $"Checking player {other.Name} {other.UserId} {other.Role.Type}");

                        if (!other.Switches.ShouldSendPosition)
                        {
                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"ShouldSendPosition=false");

                            continue;
                        }

                        if (!other.Role.Is<IFpcRole>(out _))
                        {
                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Not an IFpcRole");

                            continue;
                        }

                        if (other.NetId == player.NetId && !player.Switches.ShouldReceiveOwnPosition)
                        {
                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Same player (ShouldReceiveOwnPosition=false)");

                            continue;
                        }

                        _validBuffer.Add(other);

                        if (IsDebug)
                            ApiLoader.Debug("Position Synchronizer", $"Validated player {other.Name} {other.UserId} {other.Name}");
                    }

                    if (_validBuffer.Count < 1)
                        continue;

                    if (!_syncCache.TryGetValue(player, out var syncCache))
                        _syncCache[player] = syncCache = DictionaryPool<ExPlayer, PositionData>.Rent();

                    player.Connection.Send<FpcPositionMessage>(writer =>
                    {
                        writer.WriteUShort((ushort)_validBuffer.Count);

                        if (IsDebug)
                            ApiLoader.Debug("Position Synchronizer", $"Written count ({_validBuffer.Count}), {writer.buffer.Length} bytes");

                        for (int y = 0; y < _validBuffer.Count; y++)
                        {
                            var other = _validBuffer[y];
                            var fpcRole = (IFpcRole)other.Role.Role;

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Writing player {other.Name} {other.UserId} {other.Role.Type}");

                            var mouseLook = fpcRole.FpcModule.MouseLook;
                            var module = fpcRole.FpcModule;

                            var isInvisible = hasRole && customVisibilityRole.VisibilityController.ValidateVisibility(other.Hub);

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"1 isInvisible={isInvisible}");

                            if (!isInvisible && (ghosted || other._invisibility.Contains(player)))
                                isInvisible = true;

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"2 isInvisible={isInvisible}");

                            if (!syncCache.TryGetValue(other, out var prevData))
                                prevData = syncCache[other] = new PositionData();

                            var position = other.Transform.position;
                            var posBit = false;

                            if (other.Position.FakedList.TryGetValue(player, out var fakedPos))
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

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"relative={relative.Position} real={position} prev={prevData.Position.Position} syncH=({syncH} / {prevData.SyncH}) syncV=({syncV} / {prevData.SyncV}) posBit={posBit} lookBit={lookBit} customBit={customBit}");

                            prevData.Position = relative;

                            prevData.SyncH = syncH;
                            prevData.SyncV = syncV;

                            Misc.ByteToBools((byte)module.SyncMovementState, out var b1, out var b2, out var b3, out var b4, out var b5, out var b6, out var b7, out _);

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"b1={b1} b2={b2} b3={b3} b4={b4} b5={b5}");

                            writer.WriteRecyclablePlayerId(other.Hub.Network_playerId);

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Written ID, {writer.buffer.Length} bytes");

                            writer.WriteByte(Misc.BoolsToByte(b1, b1, b3, b4, b5, lookBit, posBit, customBit));

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Written flags, {writer.buffer.Length} bytes");

                            if (posBit)
                            {
                                writer.WriteRelativePosition(relative);

                                if (IsDebug)
                                    ApiLoader.Debug("Position Synchronizer", $"Written position, {writer.buffer.Length} bytes");
                            }
                            else
                            {
                                if (IsDebug)
                                    ApiLoader.Debug("Position Synchronizer", $"posBit is false, not writing position");
                            }

                            if (lookBit)
                            {
                                writer.WriteUShort(syncH);
                                writer.WriteUShort(syncV);

                                if (IsDebug)
                                    ApiLoader.Debug("Position Synchronizer", $"Written rotation, {writer.buffer.Length} bytes");
                            }
                            else
                            {
                                if (IsDebug)
                                    ApiLoader.Debug("Position Synchronizer", $"lookBit is false, not writing rotation");
                            }

                            if (IsDebug)
                                ApiLoader.Debug("Position Synchronizer", $"Finished writing data for {other.Name} {other.UserId} {other.Role.Type} ({writer.buffer.Length})");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Position Synchronizer", $"Caught an error while synchronizing player positions:\n{ex.ToColoredString()}");
            }

            _sending = false;
            _marker.MarkEnd();
        }

        private static void Update()
        {
            if (_sending)
                return;

            if (ForceSendNextFrame)
            {
                ForceSendNextFrame = false;

                Synchronize();
                return;
            }

            var sendRate = SendRate;

            _sendTime -= sendRate;
            _debugTime -= Time.deltaTime;

            if (_sendTime > 0f)
                return;

            _sendTime = sendRate;

            Synchronize();

            if (_debugTime <= 0f)
                _debugTime = ApiLoader.Config.ApiOptions.PositionSynchronizerOptions.PositionDebugRate;
        }

        internal static void InternalHandleRoleChange(PlayerChangedRoleArgs args)
        {
            if (_restarting || args.PreviousRole is null || args.PreviousRole is not IFpcRole)
                return;

            _marker.Clear();

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
                DictionaryPool<ExPlayer, PositionData>.Return(syncCache);

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

        [OnLoad]
        private static void InternalLoad()
        {
            TickManager.OnTick += Update;
            PlayerLeavePatch.OnLeaving += InternalHandleLeave;
        }
    }
}