using Footprinting;

using GameCore;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.API.Enums;
using LabExtended.Commands.Attributes;

using LightContainmentZoneDecontamination;

using RoundRestarting;

using UnityEngine;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8629 // Nullable value type may be null.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API
{
    /// <summary>
    /// A class used for managing the round.
    /// </summary>
    [CommandPropertyAlias("round")]
    public static class ExRound
    {
        /// <summary>
        /// Represents the status of a lock.
        /// </summary>
        public class LockStatus
        {
            private bool isRound;
            
            internal LockStatus(bool isRound)
                => this.isRound = isRound;

            /// <summary>
            /// Whether or not this lock is active.
            /// </summary>
            public bool IsActive
            {
                get => field || (isRound ? RoundSummary.RoundLock : RoundStart.LobbyLock);
                internal set => field = value;
            }
            
            /// <summary>
            /// Gets the time at which this lock was enabled.
            /// </summary>
            public DateTime Time { get; internal set; }
            
            /// <summary>
            /// Gets the footprint of the player who enabled the lock.
            /// </summary>
            public Footprint? Footprint { get; internal set; }
            
            /// <summary>
            /// Gets the player who enabled the lock.
            /// </summary>
            public ExPlayer? Player { get; internal set; }

            /// <summary>
            /// Enables the lock.
            /// </summary>
            /// <param name="enabledBy">The player who enabled the lock.</param>
            /// <returns>true if the lock was enabled</returns>
            public bool Enable(ExPlayer? enabledBy = null)
            {
                if (IsActive)
                    return false;
                
                IsActive = true;
                
                Time = DateTime.Now;
                Player = enabledBy ?? ExPlayer.Host;
                
                Footprint = Player.Footprint;

                if (isRound)
                {
                    RoundSummary.RoundLock = true;
                    RoundSummary.singleton?.CancelRoundEnding();
                }
                else
                {
                    RoundStart.LobbyLock = true;
                }

                if (!Player.IsHost)
                    ApiLog.Info("LabExtended", $"Player &3{Player.Nickname}&r (&6{Player.UserId}&r) has &2ENABLED&r " +
                                                   $"&1{(isRound ? "Round Lock" : "Lobby Lock")}&r");
                
                return true;
            }

            /// <summary>
            /// Disables the lock.
            /// </summary>
            /// <param name="disabledBy">The player who disabled the round lock.</param>
            /// <returns>true if the lock was disabled</returns>
            public bool Disable(ExPlayer? disabledBy = null)
            {
                if (!IsActive)
                    return false;
                
                IsActive = false;
                
                Time = DateTime.Now;
                
                Player = null;
                Footprint = null;

                if (isRound)
                    RoundSummary.RoundLock = false;
                else
                    RoundStart.LobbyLock = false;
                
                if (disabledBy != null && !disabledBy.IsHost)
                    ApiLog.Info("LabExtended", $"Player &3{disabledBy.Nickname}&r (&6{disabledBy.UserId}&r) has &3DISABLED&r " +
                                                   $"&1{(isRound ? "Round Lock" : "Lobby Lock")}&r");
                
                return true;
            }
        }
        
        private static Vector3? _roundStartPosition;
        private static GameObject? _roundStart;
        
        /// <summary>
        /// Gets the round's number.
        /// </summary>
        [CommandPropertyAlias("number")]
        public static short RoundNumber { get; internal set; }

        /// <summary>
        /// Gets the round's current state.
        /// </summary>
        [CommandPropertyAlias("state")]
        public static RoundState State { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the round has ended.
        /// </summary>
        [CommandPropertyAlias("isEnded")]
        public static bool IsEnded => State is RoundState.Ended;
        
        /// <summary>
        /// Gets a value indicating whether or not the round is currently ending.
        /// </summary>
        [CommandPropertyAlias("isEnding")]
        public static bool IsEnding => State is RoundState.Ending;

        /// <summary>
        /// Gets a value indicating whether the round is in progress.
        /// </summary>
        [CommandPropertyAlias("isRunning")]
        public static bool IsRunning => State is RoundState.InProgress;

        /// <summary>
        /// Gets a value indicating whether the round is waiting for players.
        /// </summary>
        [CommandPropertyAlias("isWaitingForPlayers")]
        public static bool IsWaitingForPlayers => State is RoundState.WaitingForPlayers;

        /// <summary>
        /// Gets a value indicating whether or not SCP-079 was recontained.
        /// </summary>
        [CommandPropertyAlias("isScp079Recontained")]
        public static bool IsScp079Recontained { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether or not Light Containment Zone has started decontaminating.
        /// </summary>
        [CommandPropertyAlias("isLczDecontaminated")]
        public static bool IsLczDecontaminated => DecontaminationController.Singleton?.IsDecontaminating ?? false;

        /// <summary>
        /// Gets the <see cref="GameObject"/> of the background of the waiting for players screen.
        /// </summary>
        public static GameObject? RoundStartBackground => _roundStart ??= GameObject.Find("StartRound");

        /// <summary>
        /// Gets the lock controller for round lock.
        /// </summary>
        public static LockStatus RoundLock { get; } = new(true);

        /// <summary>
        /// Gets the lock controller for lobby lock.
        /// </summary>
        public static LockStatus LobbyLock { get; } = new(false);

        /// <summary>
        /// Gets the date of this round's start.
        /// </summary>
        [CommandPropertyAlias("startTime")]
        public static DateTime StartedAt { get; internal set; }
        
        /// <summary>
        /// Gets the amount of time that has passed since the round started or <see cref="TimeSpan.Zero"/> if the round isn't running.
        /// </summary>
        [CommandPropertyAlias("duration")]
        public static TimeSpan Duration => IsRunning ? DateTime.Now - StartedAt : TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the forced decontamination status.
        /// </summary>
        [CommandPropertyAlias("decontaminationStatus")]
        public static DecontaminationController.DecontaminationStatus DecontaminationStatus
        {
            get => DecontaminationController.Singleton?.DecontaminationOverride ?? DecontaminationController.DecontaminationStatus.None;
            set => DecontaminationController.Singleton!.DecontaminationOverride = value;
        }

        /// <summary>
        /// Gets the decontamination phase.
        /// </summary>
        [CommandPropertyAlias("decontaminationPhase")]
        public static DecontaminationController.DecontaminationPhase.PhaseFunction DecontaminationPhase
        {
            get
            {
                if (DecontaminationController.Singleton is null)
                    return DecontaminationController.DecontaminationPhase.PhaseFunction.None;
                
                if (DecontaminationController.Singleton._nextPhase < 0 
                    || DecontaminationController.Singleton._nextPhase >= DecontaminationController.Singleton.DecontaminationPhases.Length)
                    return DecontaminationController.DecontaminationPhase.PhaseFunction.None;

                return DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton._nextPhase].Function;
            }
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
        [CommandPropertyAlias("isLocked")]
        public static bool IsRoundLocked
        {
            get => RoundLock.IsActive;
            set
            {
                if (value == RoundLock.IsActive)
                    return;

                if (value)
                    RoundLock.Enable();
                else
                    RoundLock.Disable();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can start.
        /// </summary>
        [CommandPropertyAlias("isLobbyLocked")]
        public static bool IsLobbyLocked
        {
            get => LobbyLock.IsActive;
            set
            {
                if (value == LobbyLock.IsActive)
                    return;

                if (value)
                    LobbyLock.Enable();
                else
                    LobbyLock.Disable();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should restart once the round ends.
        /// </summary>
        public static bool ShouldRestartOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Restart;
            set => RoundEndAction = value 
                ? ServerStatic.NextRoundAction.Restart 
                : (RoundEndAction is ServerStatic.NextRoundAction.Restart 
                    ? ServerStatic.NextRoundAction.DoNothing 
                    : ServerStatic.NextRoundAction.Shutdown);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should shut down once the round ends.
        /// </summary>
        public static bool ShouldShutdownOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Shutdown;
            set => RoundEndAction = value 
                ? ServerStatic.NextRoundAction.Shutdown 
                : (RoundEndAction is ServerStatic.NextRoundAction.Shutdown 
                    ? ServerStatic.NextRoundAction.DoNothing 
                    : ServerStatic.NextRoundAction.Restart);
        }

        /// <summary>
        /// Gets or sets the current extra target count for SCPs.
        /// </summary>
        [CommandPropertyAlias("extraTargets")]
        public static int ExtraTargetCount
        {
            get => RoundSummary.singleton?.Network_extraTargets ?? 0;
            set => RoundSummary.singleton!.Network_extraTargets = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped Class-Ds.
        /// </summary>
        [CommandPropertyAlias("escapedClassD")]
        public static int EscapedClassDs
        {
            get => RoundSummary.EscapedClassD;
            set => RoundSummary.EscapedClassD = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped scientists.
        /// </summary>
        [CommandPropertyAlias("escapedScientists")]
        public static int EscapedScientists
        {
            get => RoundSummary.EscapedScientists;
            set => RoundSummary.EscapedScientists = value;
        }

        /// <summary>
        /// Gets or sets the amount of total kills.
        /// </summary>
        [CommandPropertyAlias("kills")]
        public static int Kills
        {
            get => RoundSummary.Kills;
            set => RoundSummary.Kills = value;
        }

        /// <summary>
        /// Gets or sets the amount of SCPs that have survived the round.
        /// </summary>
        [CommandPropertyAlias("scps")]
        public static int SurvivingSCPs
        {
            get => RoundSummary.SurvivingSCPs;
            set => RoundSummary.SurvivingSCPs = value;
        }

        /// <summary>
        /// Gets or sets the amount of kills that the SCPs did.
        /// </summary>
        [CommandPropertyAlias("scpKills")]
        public static int KillsByScp
        {
            get => RoundSummary.KilledBySCPs;
            set => RoundSummary.KilledBySCPs = value;
        }

        /// <summary>
        /// Gets the amount of players that have been changed into zombies.
        /// </summary>
        [CommandPropertyAlias("zombies")]
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

        /// <summary>
        /// Hides the round ending screen.
        /// </summary>
        public static void CancelRoundEnd()
            => RoundSummary.singleton?.CancelRoundEnding();

        /// <summary>
        /// Hides the background while in "Waiting for players"
        /// </summary>
        public static void HideRoundStartBackground()
        {
            _roundStartPosition ??= RoundStartBackground.transform.position;
            _roundStart.transform.position = Vector3.zero;
        }

        /// <summary>
        /// Shows the background while in "Waiting for players"
        /// <remarks>This method CANNOT be used without first hiding the screen, it will throw a null reference exception!</remarks>
        /// </summary>
        public static void ShowRoundStartBackground()
            => _roundStart.transform.position = _roundStartPosition.Value;

        private static void OnRoundWait()
        {
            _roundStart = null;

            IsScp079Recontained = false;
            
            StartedAt = DateTime.MinValue;
            State = RoundState.WaitingForPlayers;

            RoundNumber++;
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnRoundWaiting += OnRoundWait;
        }
    }
}