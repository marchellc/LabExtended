using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabExtended.Events;
using LabExtended.Events.Round;

using LabExtended.Core;
using LabExtended.Utilities.Update;

using System.Diagnostics;
using System.ComponentModel;

namespace LabExtended.API.Custom.Gamemodes
{
    /// <summary>
    /// Base class for custom gamemodes.
    /// </summary>
    public abstract class CustomGamemode : CustomObject<CustomGamemode>
    {
        private static PlayerUpdateComponent updateComponent = PlayerUpdateComponent.Create();

        /// <summary>
        /// Gets a queue of gamemodes to be activated next.
        /// </summary>
        public static Queue<CustomGamemode> Queue { get; } = new();

        /// <summary>
        /// Gets the curently enabled gamemode.
        /// </summary>
        public static CustomGamemode? Current { get; private set; }

        /// <summary>
        /// Enables the specified custom gamemode, optionally overriding the currently active gamemode.
        /// </summary>
        /// <remarks>If a gamemode is already active and overrideCurrent is false, the method will not
        /// enable the new gamemode. If the current gamemode cannot be interrupted mid-round, the specified gamemode may
        /// be queued for later activation. If overrideCurrent is true, the current gamemode will be disabled before
        /// enabling the new one.</remarks>
        /// <param name="gamemode">The custom gamemode to enable. Cannot be null.</param>
        /// <param name="overrideCurrent">true to forcibly disable the current gamemode and enable the specified one; otherwise, false to only enable
        /// if no gamemode is currently active.</param>
        /// <returns>true if the gamemode was successfully enabled; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if gamemode is null.</exception>
        public static bool Enable(CustomGamemode gamemode, bool overrideCurrent = false)
        {
            if (gamemode is null)
                throw new ArgumentNullException(nameof(gamemode));

            if (Current != null && Current == gamemode)
                return false;

            if (Current != null 
                && !Current.CanActivateMidRound 
                && !ExRound.IsWaitingForPlayers)
            {
                if (!Queue.Contains(gamemode))
                    Queue.Enqueue(gamemode);

                return false;
            }

            if (Current != null)
            {
                if (!overrideCurrent)
                    return false;

                if (!Disable())
                    return false;
            }

            Current = gamemode;
            Current.OnEnabled();

            Current.IsActive = true;
            return true;
        }

        /// <summary>
        /// Disables the current gamemode if one is active.
        /// </summary>
        /// <remarks>If no current gamemode is active, the method performs no action and returns false.
        /// After disabling, the current gamemode is set to null and will no longer be considered active.</remarks>
        /// <returns>true if an active gamemode was disabled; otherwise, false.</returns>
        public static bool Disable()
        {
            if (Current == null)
                return false;

            var current = Current;

            Current = null;

            current.IsActive = false;
            current.OnDisabled();

            return true;
        }

        private Stopwatch runTimeWatch = new();

        /// <summary>
        /// Whether or not the gamemode is currently active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game mode can be activated after a round has already started.
        /// </summary>
        [Description("Whether or not the gamemode can be started in the middle of the round.")]
        public virtual bool CanActivateMidRound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gamemode prevents the default wave spawns from occurring.
        /// </summary>
        [Description("Whether or not the gamemode prevents the default wave spawns from occurring.")]
        public virtual bool PreventWaveSpawns { get; set; } = true;

        /// <summary>
        /// Gets the amount of time that has elapsed since the gamemode has started.
        /// </summary>
        public TimeSpan RunTime => runTimeWatch.Elapsed;

        /// <summary>
        /// Gets called when the gamemode is enabled.
        /// </summary>
        public virtual void OnEnabled()
        {
            runTimeWatch.Restart();
        }

        /// <summary>
        /// Gets called when the gamemode is disabled.
        /// </summary>
        public virtual void OnDisabled()
        {
            runTimeWatch.Stop();
            runTimeWatch.Reset();
        }

        /// <inheritdoc/>
        public override void OnRegistered()
        {
            base.OnRegistered();

            ApiLog.Info("Custom Gamemodes", $"&2Registered&r custom gamemode &3{Id}&r");
        }

        /// <inheritdoc/>
        public override void OnUnregistered()
        {
            base.OnUnregistered();

            if (Current != null && Current == this)
                Disable();

            ApiLog.Info("Custom Gamemodes", $"&1Unregistered&r custom gamemode &3{Id}&r");
        }

        /// <summary>
        /// Gets called once per frame.
        /// </summary>
        public virtual void OnUpdate()
        {

        }

        /// <summary>
        /// Gets called before a round end is triggered.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundCheckingEndConditions(RoundEndingConditionsCheckEventArgs args)
        {

        }


        /// <summary>
        /// Gets called when the round is about to end.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundEnding(RoundEndingEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when the round ends.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundEnded(RoundEndedEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when the round starts restarting.
        /// </summary>
        public virtual void OnRoundRestarting()
        {

        }

        /// <summary>
        /// Gets called when the round restart finishes and the server is ready for player connections.
        /// </summary>
        public virtual void OnWaitingForPlayers()
        {

        }

        /// <summary>
        /// Gets called when the round starts.
        /// </summary>
        public virtual void OnRoundStarted()
        {

        }

        /// <summary>
        /// Gets called before player's round-start roles are assigned. 
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnAssigningRoles(AssigningRolesEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after player's round-start roles are assigned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnAssignedRoles(AssignedRolesEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when a player is setting their role while late joining.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnLateJoinSettingRole(LateJoinSettingRoleEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after a player's role is set by late-join.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnLateJoinSetRole(LateJoinSetRoleEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when a new player joins the server.
        /// </summary>
        /// <param name="player">The player who joined.</param>
        public virtual void OnPlayerJoined(ExPlayer player)
        {

        }

        /// <summary>
        /// Gets called when a player leaves the server.
        /// </summary>
        /// <param name="player">The player who left.</param>
        public virtual void OnPlayerLeft(ExPlayer player)
        {

        }

        /// <summary>
        /// Gets called before a new reinforcement wave is selected.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSelecting(WaveTeamSelectingEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after a new reinforcement wave is selected.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSelected(WaveTeamSelectedEventArgs args)
        {

        }

        /// <summary>
        /// Gets called before a new reinforcement wave is spawned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSpawning(WaveRespawningEventArgs args)
        {
            if (PreventWaveSpawns)
                args.IsAllowed = false;
        }

        /// <summary>
        /// Gets called after a new reinforcement wave has been spawned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSpawned(WaveRespawnedEventArgs args)
        {

        }

        private static void _WaitingForPlayers()
        {
            Current?.OnWaitingForPlayers();

            if (Current == null && Queue.TryDequeue(out var gamemode))
                Enable(gamemode);
        }

        private static void _Update()
        {
            Current?.OnUpdate();
        }

        private static void _RoundCheckingEndConditions(RoundEndingConditionsCheckEventArgs args)
        {
            Current?.OnRoundCheckingEndConditions(args);
        }

        private static void _RoundEnding(RoundEndingEventArgs args)
        {
            Current?.OnRoundEnding(args);
        }

        private static void _RoundEnded(RoundEndedEventArgs args)
        {
            Current?.OnRoundEnded(args);
        }

        private static void _RoundRestarting()
        {
            Current?.OnRoundRestarting();
        }

        private static void _RoundStarted()
        {
            Current?.OnRoundStarted();
        }

        private static void _AssigningRoles(AssigningRolesEventArgs args)
        {
            Current?.OnAssigningRoles(args);
        }

        private static void _AssignedRoles(AssignedRolesEventArgs args)
        {
            Current?.OnAssignedRoles(args);
        }

        private static void _LateJoinSettingRole(LateJoinSettingRoleEventArgs args)
        {
            Current?.OnLateJoinSettingRole(args);
        }

        private static void _LateJoinSetRole(LateJoinSetRoleEventArgs args)
        {
            Current?.OnLateJoinSetRole(args);
        }

        private static void _PlayerJoined(ExPlayer player)
        {
            Current?.OnPlayerJoined(player);
        }

        private static void _PlayerLeft(ExPlayer player)
        {
            Current?.OnPlayerLeft(player);
        }

        private static void _WaveSelecting(WaveTeamSelectingEventArgs args)
        {
            Current?.OnWaveSelecting(args);
        }

        private static void _WaveSelected(WaveTeamSelectedEventArgs args)
        {
            Current?.OnWaveSelected(args);
        }

        private static void _WaveSpawning(WaveRespawningEventArgs args)
        {
            Current?.OnWaveSpawning(args);
        }

        private static void _WaveSpawned(WaveRespawnedEventArgs args)
        {
            Current?.OnWaveSpawned(args);
        }

        internal static void _Init()
        {
            updateComponent.OnUpdate += _Update;

            ExRoundEvents.AssignedRoles += _AssignedRoles;
            ExRoundEvents.AssigningRoles += _AssigningRoles;

            ExRoundEvents.LateJoinSetRole += _LateJoinSetRole;
            ExRoundEvents.LateJoinSettingRole += _LateJoinSettingRole;

            ExRoundEvents.Started += _RoundStarted;
            ExRoundEvents.Restarting += _RoundRestarting;
            ExRoundEvents.WaitingForPlayers += _WaitingForPlayers;

            ExPlayerEvents.Left += _PlayerLeft;
            ExPlayerEvents.Verified += _PlayerJoined;

            ServerEvents.WaveRespawned += _WaveSpawned;
            ServerEvents.WaveRespawning += _WaveSpawning;

            ServerEvents.WaveTeamSelected += _WaveSelected;
            ServerEvents.WaveTeamSelecting += _WaveSelecting;

            ServerEvents.RoundEnded += _RoundEnded;
            ServerEvents.RoundEnding += _RoundEnding;

            ServerEvents.RoundEndingConditionsCheck += _RoundCheckingEndConditions;
        }
    }
}