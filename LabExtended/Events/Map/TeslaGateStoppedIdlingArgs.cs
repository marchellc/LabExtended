using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate stops idling.
    /// </summary>
    public class TeslaGateStoppedIdlingArgs : IHookEvent
    {
        /// <summary>
        /// Gets the tesla gate that stopped idling.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        internal TeslaGateStoppedIdlingArgs(API.ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}