namespace LabExtended.Modules
{
    /// <summary>
    /// Specifies how a module tick behaves.
    /// </summary>
    public enum ModuleTickType
    {
        /// <summary>
        /// Ticks are called once every frame.
        /// </summary>
        OnUpdate,

        /// <summary>
        /// Ticks are synchronized with frame time.
        /// </summary>
        Synchronized,

        /// <summary>
        /// Delay between each tick is randomized.
        /// </summary>
        Randomized,

        /// <summary>
        /// Delay between each tick is static.
        /// </summary>
        Fixed,
    }
}