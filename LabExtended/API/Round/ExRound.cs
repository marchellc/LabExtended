using Footprinting;

using RoundRestarting;

namespace LabExtended.API.Round
{
    /// <summary>
    /// A class used for managing the round.
    /// </summary>
    public static class ExRound
    {
        private static RoundLock? _roundLockTracking;
        private static RoundLock? _lobbyLockTracking;

        /// <summary>
        /// Gets the round's number.
        /// </summary>
        public static short RoundNumber { get; internal set; }

        /// <summary>
        /// Gets the round's current state.
        /// </summary>
        public static RoundState State { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable the round lock if the player who enabled it leaves.
        /// </summary>
        public static bool DisableRoundLockOnLeave { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable the lobby lock if the player who enabled it leaves.
        /// </summary>
        public static bool DisableLobbyLockOnLeave { get; set; }

        /// <summary>
        /// Gets a value indicating whether the round is ending.
        /// </summary>
        public static bool IsEnding => State is RoundState.Ending;

        /// <summary>
        /// Gets a value indicating whether the round has ended.
        /// </summary>
        public static bool IsEnded => State is RoundState.Ended;

        /// <summary>
        /// Gets a value indicating whether the round is in progress.
        /// </summary>
        public static bool IsRunning => State is RoundState.InProgress || State is RoundState.Starting;

        /// <summary>
        /// Gets a value indicating whether the round is starting.
        /// </summary>
        public static bool IsStarting => State is RoundState.Starting;

        /// <summary>
        /// Gets a value indicating whether the round is waiting for players.
        /// </summary>
        public static bool IsWaitingForPlayers => State is RoundState.WaitingForPlayers;

        /// <summary>
        /// Gets the date of this round's start.
        /// </summary>
        public static DateTime StartedAt { get; internal set; }

        /// <summary>
        /// Gets the date of the round lock being enabled or <see cref="DateTime.MinValue"/> if the round lock is disabled.
        /// </summary>
        public static DateTime RoundLockEnabledAt => _roundLockTracking?.EnabledAt ?? DateTime.MinValue;

        /// <summary>
        /// Gets the date of the lobby lock being enabled or <see cref="DateTime.MinValue"/> if the lobby lock is disabled.
        /// </summary>
        public static DateTime LobbyLockEnabledAt => _lobbyLockTracking?.EnabledAt ?? DateTime.MinValue;

        /// <summary>
        /// Gets the amount of time that has passed since the round lock was enabled or <see cref="TimeSpan.Zero"/> if the round lock is disabled.
        /// </summary>
        public static TimeSpan TimePassedSinceRoundLock => _roundLockTracking.HasValue ? DateTime.Now - _roundLockTracking.Value.EnabledAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the amount of time that has passed since the lobby lock was enabled or <see cref="TimeSpan.Zero"/> if the lobby lock is disabled.
        /// </summary>
        public static TimeSpan TimePassedSinceLobbyLock => _lobbyLockTracking.HasValue ? DateTime.Now - _lobbyLockTracking.Value.EnabledAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the amount of time that has passed since the round started or <see cref="TimeSpan.Zero"/> if the round isn't running.
        /// </summary>
        public static TimeSpan Duration => IsRunning ? DateTime.Now - StartedAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the player who enabled the round lock or <see langword="null"/> if the round lock is disabled.
        /// </summary>
        public static ExPlayer RoundLockEnabledBy => _roundLockTracking?.EnabledBy ?? null;

        /// <summary>
        /// Gets the player who enabled the lobby lock or <see langword="null"/> if the lobby lock is disabled.
        /// </summary>
        public static ExPlayer LobbyLockEnabledBy => _lobbyLockTracking?.EnabledBy ?? null;

        /// <summary>
        /// Gets the <see cref="Footprint"/> of the player who enabled the round lock.
        /// </summary>
        public static Footprint RoundLockEnabledByFootprint => _roundLockTracking?.EnabledByFootprint ?? default;

        /// <summary>
        /// Gets the <see cref="Footprint"/> of the player who enabled the lobby lock.
        /// </summary>
        public static Footprint LobbyLockEnabledByFootpring => _lobbyLockTracking?.EnabledByFootprint ?? default;

        /// <summary>
        /// Gets or sets the round lock tracker.
        /// </summary>
        public static RoundLock? RoundLock
        {
            get => _roundLockTracking;
            set => _roundLockTracking = value;
        }

        /// <summary>
        /// Gets or sets the lobbby lock tracker.
        /// </summary>
        public static RoundLock? LobbyLock
        {
            get => _lobbyLockTracking;
            set => _lobbyLockTracking = value;
        }

        /// <summary>
        /// Gets or sets the action to do when the round ends.
        /// </summary>
        public static ServerStatic.NextRoundAction RoundEndAction
        {
            get => ServerStatic.StopNextRound;
            set => ServerStatic.StopNextRound = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can end.
        /// </summary>
        public static bool IsRoundLocked
        {
            get => _roundLockTracking.HasValue;
            set => _roundLockTracking = value ? new RoundLock(ExPlayer.Host) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can start.
        /// </summary>
        public static bool IsLobbyLocked
        {
            get => _lobbyLockTracking.HasValue;
            set => _lobbyLockTracking = value ? new RoundLock(ExPlayer.Host) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should restart once the round ends.
        /// </summary>
        public static bool ShouldRestartOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Restart;
            set => RoundEndAction = RoundEndAction is ServerStatic.NextRoundAction.Restart ? ServerStatic.NextRoundAction.DoNothing : ServerStatic.NextRoundAction.Shutdown;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should shut down once the round ends.
        /// </summary>
        public static bool ShouldShutdownOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Shutdown;
            set => RoundEndAction = RoundEndAction is ServerStatic.NextRoundAction.Shutdown ? ServerStatic.NextRoundAction.DoNothing : ServerStatic.NextRoundAction.Restart;
        }

        /// <summary>
        /// Gets or sets the current's Chaos Insurgency target count.
        /// </summary>
        public static int ChaosTargetCount
        {
            get => RoundSummary.singleton?.Network_chaosTargetCount ?? 0;
            set => RoundSummary.singleton!.Network_chaosTargetCount = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped Class-Ds.
        /// </summary>
        public static int EscapedClassDs
        {
            get => RoundSummary.EscapedClassD;
            set => RoundSummary.EscapedClassD = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped scientists.
        /// </summary>
        public static int EscapedScientists
        {
            get => RoundSummary.EscapedScientists;
            set => RoundSummary.EscapedScientists = value;
        }

        /// <summary>
        /// Gets or sets the amount of total kills.
        /// </summary>
        public static int Kills
        {
            get => RoundSummary.Kills;
            set => RoundSummary.Kills = value;
        }

        /// <summary>
        /// Gets or sets the amount of SCPs that have survived the round.
        /// </summary>
        public static int SurvivingSCPs
        {
            get => RoundSummary.SurvivingSCPs;
            set => RoundSummary.SurvivingSCPs = value;
        }

        /// <summary>
        /// Gets or sets the amount of kills that the SCPs did.
        /// </summary>
        public static int KillsByScp
        {
            get => RoundSummary.KilledBySCPs;
            set => RoundSummary.KilledBySCPs = value;
        }

        /// <summary>
        /// Gets the amount of players that have been changed into zombies.
        /// </summary>
        public static int ChangedIntoZombies
        {
            get => RoundSummary.ChangedIntoZombies;
            set => RoundSummary.ChangedIntoZombies = value;
        }

        /// <summary>
        /// Forces the round to start.
        /// </summary>
        public static void ForceStart()
            => CharacterClassManager.ForceRoundStart();

        /// <summary>
        /// Forces the round to end.
        /// </summary>
        public static void ForceEnd()
            => RoundSummary.singleton?.ForceEnd();

        /// <summary>
        /// Restarts the round.
        /// </summary>
        /// <param name="fastRestart">Whether or not to use fast restart.</param>
        /// <param name="restartAction">The action to perform.</param>
        public static void Restart(bool fastRestart = true, ServerStatic.NextRoundAction? restartAction = null)
        {
            if (restartAction.HasValue)
                RoundEndAction = restartAction.Value;

            var oldValue = CustomNetworkManager.EnableFastRestart;

            CustomNetworkManager.EnableFastRestart = fastRestart;
            RoundRestart.InitiateRoundRestart();
            CustomNetworkManager.EnableFastRestart = oldValue;
        }

        /// <summary>
        /// Restarts the round silently (using fast restart).
        /// </summary>
        public static void RestartSilently()
            => Restart(true, ServerStatic.NextRoundAction.DoNothing);
    }
}