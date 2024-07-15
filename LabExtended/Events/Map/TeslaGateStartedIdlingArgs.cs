using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate starts idling.
    /// </summary>
    public class TeslaGateStartedIdlingArgs : IHookEvent
    {
        /// <summary>
        /// Gets the tesla gate that started idling.
        /// </summary>
        public ExTeslaGate Gate { get; }

        internal TeslaGateStartedIdlingArgs(ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}