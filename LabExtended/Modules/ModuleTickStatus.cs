namespace LabExtended.Modules
{
    /// <summary>
    /// Specifies the current status of a module's tick.
    /// </summary>
    public enum ModuleTickStatus
    {
        /// <summary>
        /// The tick cannot execute yet.
        /// </summary>
        Paused,

        /// <summary>
        /// The tick is currently executing.
        /// </summary>
        Alive
    }
}