using LabExtended.API.Map;
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
        public ExTeslaGate Gate { get; }

        internal TeslaGateStoppedIdlingArgs(ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}