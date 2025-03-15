using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player triggers a tesla gate.
    /// <remarks>This event is called only once the Tesla is initially triggered.</remarks>
    /// </summary>
    public class PlayerTriggeringTeslaGateEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the player who triggered the tesla gate.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the tesla gate that was triggered.
        /// </summary>
        public ExTeslaGate Gate { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerTriggeringTeslaGateEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player triggering a gate.</param>
        /// <param name="gate">The gate being triggered.</param>
        public PlayerTriggeringTeslaGateEventArgs(ExPlayer player, ExTeslaGate gate)
        {
            Player = player;
            Gate = gate;
        }
    }
}