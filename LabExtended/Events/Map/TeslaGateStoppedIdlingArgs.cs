namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate stops idling.
    /// </summary>
    public class TeslaGateStoppedIdlingArgs
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