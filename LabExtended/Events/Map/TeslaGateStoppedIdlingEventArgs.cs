namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate stops idling.
    /// </summary>
    public class TeslaGateStoppedIdlingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the tesla gate that stopped idling.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        /// <summary>
        /// Creates a new <see cref="TeslaGateStoppedIdlingEventArgs"/> instance.
        /// </summary>
        /// <param name="gate">The gate that stopped idling.</param>
        public TeslaGateStoppedIdlingEventArgs(API.ExTeslaGate gate)
        {
            Gate = gate;
        }
    }
}