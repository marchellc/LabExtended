using LabApi.Events;
using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core;

using LabExtended.Attributes;
using LabExtended.Extensions;

using NetworkManagerUtils;
using NetworkManagerUtils.Dummies;

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static event Action? OnRoundRestart;
        internal static event Action? OnRoundWaiting;
        internal static event Action? OnRoundStarted;
        internal static event Action? OnRoundEnded;

        internal static event Action<ExPlayer>? OnPlayerVerified; 
        internal static event Action<ExPlayer>? OnPlayerJoined;
        internal static event Action<ExPlayer>? OnPlayerLeft; 
        
        internal static event Action<PlayerChangedRoleEventArgs>? OnRoleChanged;
        internal static event Action<PlayerSpawningEventArgs>? OnSpawning;

        internal static void HandlePlayerVerified(ExPlayer player)
        {
            if (player.IsServer)
                return;
            
            OnPlayerVerified.InvokeSafe(player);
            
            if (player.IsNpc) 
                return;

            if (string.IsNullOrWhiteSpace(player.CountryCode)
                && !string.IsNullOrWhiteSpace(player.UserId)
                && ExPlayer.preauthData.TryGetValue(player.UserId, out var region))
                player.CountryCode = region;
            
            ApiLog.Info("LabExtended",
                $"Player &3{player.Nickname}&r (&6{player.UserId}&r) &2joined&r from &3{player.IpAddress} ({player.CountryCode})&r!");
            
            ExPlayerEvents.OnVerified(player);
        }

        internal static void HandlePlayerJoin(ExPlayer player)
        {
            if (!player.IsServer)
                OnPlayerJoined.InvokeSafe(player);

            if (!player.IsNpc) 
                ExPlayerEvents.OnJoined(player);
        }

        internal static void HandlePlayerLeave(ExPlayer? player)
        {
            if (ExRound.State is RoundState.InProgress || ExRound.State is RoundState.WaitingForPlayers)
            {
                if (ApiLoader.BaseConfig.DisableRoundLockOnLeave && ExRound.RoundLock.HasValue && ExRound.RoundLock.Value.EnabledBy == player)
                {
                    ExRound.IsRoundLocked = false;
                    ApiLog.Warn("Round API", $"Round Lock disabled - the player who enabled it (&3{player.Nickname}&r &6{player.UserId}&r) left the server.");
                }

                if (ApiLoader.BaseConfig.DisableLobbyLockOnLeave && ExRound.LobbyLock.HasValue && ExRound.LobbyLock.Value.EnabledBy == player)
                {
                    ExRound.IsLobbyLocked = false;
                    ApiLog.Warn("Round API", $"Lobby Lock disabled - the player who enabled it (&3{player.Nickname}&r &6{player.UserId}&r) left the server.");
                }
            }
            
            OnPlayerLeft.InvokeSafe(player);

            if (player is { IsServer: false, IsNpc: false })
                ApiLog.Info("LabExtended", $"Player &3{player.Nickname}&r (&3{player.UserId}&r) &1left&r from &3{player.IpAddress}&r!");
            
            ExPlayerEvents.OnLeft(player);
        }

        private static void HandleSpawning(PlayerSpawningEventArgs args)
            => OnSpawning.InvokeSafe(args);

        private static void HandleRoleChange(PlayerChangedRoleEventArgs args)
            => OnRoleChanged.InvokeSafe(args);
        
        private static void HandlePlayerAuth(PlayerPreAuthenticatingEventArgs ev)
            => ExPlayer.preauthData[ev.UserId] = ev.Region;
        
        private static void HandleRoundWaiting()
        {
            ExPlayer.preauthData.Clear();

            // No reason not to reset the NPC connection ID
            DummyNetworkConnection._idGenerator = ushort.MaxValue;

            OnRoundWaiting.InvokeSafe();
            
            ExRoundEvents.OnWaitingForPlayers();
        }

        private static void HandleRoundRestart()
        {
            ExRound.State = RoundState.Restarting;
            
            OnRoundRestart.InvokeSafe();
            
            ExRoundEvents.OnRestarting();
        }

        private static void HandleRoundStart()
        {
            ExRound.StartedAt = DateTime.Now;
            ExRound.State = RoundState.InProgress;

            OnRoundStarted.InvokeSafe();
            
            ExRoundEvents.OnStarted();
        }

        private static void HandleRoundEnding(RoundEndingEventArgs ev)
        {
            ExRound.State = RoundState.Ending;
            ExRoundEvents.OnEnding();
        }

        private static void HandleRoundEnd(RoundEndedEventArgs _)
        {
            ExRound.State = RoundState.Ended;

            OnRoundEnded.InvokeSafe();
            
            ExRoundEvents.OnEnded();
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            var plyEvents = typeof(PlayerEvents);
            var srvEvents = typeof(ServerEvents);
            
            plyEvents.InsertFirst<LabEventHandler<PlayerPreAuthenticatingEventArgs>>(nameof(PlayerEvents.PreAuthenticating), HandlePlayerAuth);
            plyEvents.InsertFirst<LabEventHandler<PlayerChangedRoleEventArgs>>(nameof(PlayerEvents.ChangedRole), HandleRoleChange);
            plyEvents.InsertFirst<LabEventHandler<PlayerSpawningEventArgs>>(nameof(PlayerEvents.Spawning), HandleSpawning);
            
            srvEvents.InsertFirst<LabEventHandler<RoundEndingEventArgs>>(nameof(ServerEvents.RoundEnding), HandleRoundEnding);
            srvEvents.InsertFirst<LabEventHandler<RoundEndedEventArgs>>(nameof(ServerEvents.RoundEnded), HandleRoundEnd);
            srvEvents.InsertFirst<LabEventHandler>(nameof(ServerEvents.WaitingForPlayers), HandleRoundWaiting);
            srvEvents.InsertFirst<LabEventHandler>(nameof(ServerEvents.RoundRestarted), HandleRoundRestart);
            srvEvents.InsertFirst<LabEventHandler>(nameof(ServerEvents.RoundStarted), HandleRoundStart);
        }
    }
}