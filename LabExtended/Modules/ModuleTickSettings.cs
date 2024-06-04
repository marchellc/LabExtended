namespace LabExtended.Modules
{
    /// <summary>
    /// Configures how a module's tick behaves.
    /// </summary>
    public struct ModuleTickSettings
    {
        /// <summary>
        /// The minimum delay between each tick (used for the <see cref="ModuleTickType.Randomized"/> tick type).
        /// </summary>
        public readonly float? MinTime;

        /// <summary>
        /// The maximum delay between each tick (used for the <see cref="ModuleTickType.Randomized"/> tick type).
        /// </summary>
        public readonly float? MaxTime;

        /// <summary>
        /// The static delay between each tick (used for the <see cref="ModuleTickType.Fixed"/> tick type).
        /// </summary>
        public readonly float? FixedTime;

        /// <summary>
        /// Amount of frames that needs to pass between each tick (used for the <see cref="ModuleTickType.OnUpdate"/> tick type).
        /// </summary>
        public readonly int? FrameCount;

        /// <summary>
        /// The module's preferred tick type.
        /// </summary>
        public readonly ModuleTickType TickType;

        /// <summary>
        /// Creates a new tick configuration using the <see cref="ModuleTickType.Fixed"/> tick type.
        /// </summary>
        /// <param name="fixedTime">The static delay between each tick (in milliseconds).</param>
        public ModuleTickSettings(float fixedTime)
        {
            MinTime = null;
            MaxTime = null;
            FrameCount = null;

            FixedTime = fixedTime;

            TickType = ModuleTickType.Fixed;
        }

        /// <summary>
        /// Creates a new tick configuration using the <see cref="ModuleTickType.Randomized"/> tick type.
        /// </summary>
        /// <param name="minTime">The minimum delay between each tick (in milliseconds).</param>
        /// <param name="maxTime">The maximum delay between each tick (in milliseconds).</param>
        public ModuleTickSettings(float minTime, float maxTime)
        {
            MinTime = minTime;
            MaxTime = maxTime;

            TickType = ModuleTickType.Randomized;

            FrameCount = null;
            FixedTime = null;
        }

        /// <summary>
        /// Creates a new tick configuration using the <see cref="ModuleTickType.OnUpdate"/> tick type.
        /// </summary>
        /// <param name="frameCount">The amount of frames that needs to pass between each tick.</param>
        public ModuleTickSettings(int frameCount = 0)
        {
            FrameCount = frameCount;

            TickType = ModuleTickType.OnUpdate;

            MinTime = null;
            MaxTime = null;
            FixedTime = null;
        }

        internal ModuleTickSettings(ModuleTickType tickType, float? minTime, float? maxTime, float? fixedTime, int? frameCount)
        {
            TickType = tickType;
            MinTime = minTime;
            MaxTime = maxTime;
            FixedTime = fixedTime;
            FrameCount = frameCount;
        }
    }
}