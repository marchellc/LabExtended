namespace LabExtended.Enums
{
    /// <summary>
    /// Specifies a Tesla Gate's current state.
    /// </summary>
    public enum TeslaState
    {
        /// <summary>
        /// The Tesla Gate is idling.
        /// </summary>
        Idle,

        /// <summary>
        /// The Tesla Gate is preparing to burst.
        /// </summary>
        Charging,

        /// <summary>
        /// The Tesla Gate is bursting.
        /// </summary>
        Bursting
    }
}