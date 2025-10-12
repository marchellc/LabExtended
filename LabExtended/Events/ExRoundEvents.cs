using LabExtended.Events.Round;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events
{
    /// <summary>
    /// A class used for round event delegates. These delegates get called before any other event handlers.
    /// </summary>
    public static class ExRoundEvents
    {
        /// <summary>
        /// Gets called when the round ends.
        /// </summary>
        public static event Action? Ended;

        /// <summary>
        /// Gets called when the round's end screen appears.
        /// </summary>
        public static event Action? Ending; 

        /// <summary>
        /// Gets called when the round starts.
        /// </summary>
        public static event Action? Started;

        /// <summary>
        /// Gets called when the round starts restarting.
        /// </summary>
        public static event Action? Restarting;

        /// <summary>
        /// Gets called when the round starts waiting for players.
        /// </summary>
        public static event Action? WaitingForPlayers;
        
        /// <summary>
        /// Gets called when roles are being assigned at the start of the round.
        /// </summary>
        public static event Action<AssigningRolesEventArgs>? AssigningRoles;

        /// <summary>
        /// Gets called after player's round-start roles are assigned.
        /// </summary>
        public static event Action<AssignedRolesEventArgs>? AssignedRoles;

        /// <summary>
        /// Gets called before a player's role is set by late-join.
        /// </summary>
        public static event Action<LateJoinSettingRoleEventArgs>? LateJoinSettingRole;

        /// <summary>
        /// Gets called after a player's role is set by late-join.
        /// </summary>
        public static event Action<LateJoinSetRoleEventArgs>? LateJoinSetRole;

        /// <summary>
        /// Invokes the <see cref="Ended"/> event.
        /// </summary>
        public static void OnEnded()
            => Ended.InvokeEvent("Ended");
        
        /// <summary>
        /// Invokes the <see cref="Ending"/> event.
        /// </summary>
        public static void OnEnding()
            => Ending.InvokeEvent("Ending");
        
        /// <summary>
        /// Invokes the <see cref="Started"/> event.
        /// </summary>
        public static void OnStarted()
            => Started.InvokeEvent("Started");

        /// <summary>
        /// Invokes the <see cref="Restarting"/> event.
        /// </summary>
        public static void OnRestarting()
            => Restarting.InvokeEvent("Restarting");
        
        /// <summary>
        /// Invokes the <see cref="WaitingForPlayers"/> event.
        /// </summary>
        public static void OnWaitingForPlayers()
            => WaitingForPlayers.InvokeEvent("WaitingForPlayers");
        
        /// <summary>
        /// Invokes the <see cref="AssigningRoles"/> event.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public static bool OnAssigningRoles(AssigningRolesEventArgs args)
            => AssigningRoles.InvokeBooleanEvent(args);

        /// <summary>
        /// Invokes the <see cref="AssignedRoles"/> event.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public static void OnAssignedRoles(AssignedRolesEventArgs args)
            => AssignedRoles.InvokeEvent(args);

        /// <summary>
        /// Invokes the <see cref="LateJoinSettingRole"/> event.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public static bool OnLateJoinSettingRole(LateJoinSettingRoleEventArgs args)
            => LateJoinSettingRole.InvokeBooleanEvent(args);

        /// <summary>
        /// Invokes the <see cref="LateJoinSetRole"/> event.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public static void OnLateJoinSetRole(LateJoinSetRoleEventArgs args)
            => LateJoinSetRole.InvokeEvent(args);
    }
}