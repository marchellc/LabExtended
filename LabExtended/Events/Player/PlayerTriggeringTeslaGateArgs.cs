using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player triggers a tesla gate.
    /// <para>Unlike other frameworks, this event triggers ONLY if the Tesla Gate is not already active.</para>
    /// </summary>
    public class PlayerTriggeringTeslaGateArgs : BoolCancellableEvent
    {
        /// <summary>
        /// Gets the player who triggered the tesla gate.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the tesla gate.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        internal PlayerTriggeringTeslaGateArgs(ExPlayer player, API.ExTeslaGate gate)
        {
            Player = player;
            Gate = gate;
        }
    }
}