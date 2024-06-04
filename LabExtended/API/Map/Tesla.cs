using LabExtended.Enums;
using LabExtended.Utilities;

namespace LabExtended.API.Map
{
    /// <summary>
    /// A <see cref="TeslaGate"/> wrapper.
    /// </summary>
    public class Tesla : NetworkedWrapper<TeslaGate>
    {
        public Tesla(TeslaGate baseValue) : base(baseValue) { }

        /// <summary>
        /// Get's the Tesla Gate's current state.
        /// </summary>
        public TeslaState State { get; }
    }
}