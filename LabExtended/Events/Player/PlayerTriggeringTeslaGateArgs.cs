using LabExtended.API;
using LabExtended.API.Map;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player triggers a tesla gate.
    /// <para>Unlike other frameworks, this event triggers ONLY if the Tesla Gate is not already active.</para>
    /// </summary>
    public class PlayerTriggeringTeslaGateArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the player who triggered the tesla gate.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the tesla gate.
        /// </summary>
        public ExTeslaGate Gate { get; }

        internal PlayerTriggeringTeslaGateArgs(ExPlayer player, ExTeslaGate gate)
        {
            Player = player;
            Gate = gate;
        }
    }
}