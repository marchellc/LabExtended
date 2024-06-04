namespace LabExtended.Modules
{
    /// <summary>
    /// An internal classed used to keep track of module ticks.
    /// </summary>
    public class ModuleTickStatusInfo
    {
        private int _passedFrames = 0;

        /// <summary>
        /// Gets the current tick status.
        /// </summary>
        public ModuleTickStatus Status { get; private set; }

        /// <summary>
        /// Gets the time of the last tick.
        /// </summary>
        public DateTime LastTick { get; private set; }

        /// <summary>
        /// Gets the time of the next tick.
        /// </summary>
        public DateTime NextTick { get; private set; }

        /// <summary>
        /// Gets the module's tick settings.
        /// </summary>
        public ModuleTickSettings TickSettings { get; }

        internal ModuleTickStatusInfo(ModuleTickSettings moduleTickSettings)
            => TickSettings = moduleTickSettings;

        /// <summary>
        /// Returns a value indicating whether or not a tick can occur.
        /// </summary>
        /// <returns>A value indicating whether or not a tick can occur.</returns>
        public bool CanTick()
        {
            if (TickSettings.FrameCount.HasValue && _passedFrames >= TickSettings.FrameCount.Value)
            {
                _passedFrames = 0;
                return true;
            }
            else if (TickSettings.FrameCount.HasValue)
            {
                _passedFrames++;
                return false;
            }
            else if (TickSettings.TickType is ModuleTickType.Fixed || TickSettings.TickType is ModuleTickType.Randomized)
            {
                return DateTime.Now >= NextTick;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Method called before a tick is executed, essentially just to set the tick status.
        /// </summary>
        public void PreTick()
            => Status = ModuleTickStatus.Alive;

        /// <summary>
        /// Method called after a tick is executed, used to set the next tick time.
        /// </summary>
        public void PostTick()
        {
            LastTick = DateTime.Now;
            Status = ModuleTickStatus.Paused;

            if (TickSettings.TickType is ModuleTickType.Fixed)
                NextTick = LastTick.AddMilliseconds(TickSettings.FixedTime.Value);
            else if (TickSettings.TickType is ModuleTickType.Randomized)
                NextTick = LastTick.AddMilliseconds(RandomGenerator.GetFloatUnsecure(TickSettings.MinTime.Value, TickSettings.MaxTime.Value));
        }
    }
}