namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate starts triggering.
    /// </summary>
    public class TeslaGateTriggeringEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the tesla gate that started triggering.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the shock should be instant.
        /// </summary>
        public bool IsInstant { get; set; }

        /// <summary>
        /// Creates a new <see cref="TeslaGateTriggeringEventArgs"/> instance.
        /// </summary>
        /// <param name="gate">The gate that's being triggered.</param>
        /// <param name="isInstant">Should the shock be instant?</param>
        public TeslaGateTriggeringEventArgs(API.ExTeslaGate gate, bool isInstant)
        {
            Gate = gate;
            IsInstant = isInstant;
        }
    }
}