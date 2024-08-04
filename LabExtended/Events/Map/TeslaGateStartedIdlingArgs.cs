namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate starts idling.
    /// </summary>
    public class TeslaGateStartedIdlingArgs
    {
        /// <summary>
        /// Gets the tesla gate that started idling.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        internal TeslaGateStartedIdlingArgs(API.ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}