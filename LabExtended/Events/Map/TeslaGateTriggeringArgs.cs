using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a tesla gate starts triggering.
    /// </summary>
    public class TeslaGateTriggeringArgs : BoolCancellableEvent
    {
        /// <summary>
        /// Gets the tesla gate that started triggering.
        /// </summary>
        public API.ExTeslaGate Gate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the shock should be instant.
        /// </summary>
        public bool IsInstant { get; set; }

        internal TeslaGateTriggeringArgs(API.ExTeslaGate gate, bool isInstant)
        {
            Gate = gate;
            IsInstant = isInstant;
        }
    }
}