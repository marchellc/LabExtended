namespace LabExtended.API.Modules
{
    public class ModuleTickStatusInfo
    {
        private int _passedFrames = 0;

        public ModuleTickStatus Status { get; private set; }

        public DateTime LastTick { get; private set; }
        public DateTime NextTick { get; private set; }

        public ModuleTickSettings TickSettings { get; }

        public ModuleTickStatusInfo(ModuleTickSettings moduleTickSettings)
        {
            TickSettings = moduleTickSettings;
        }

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

        public void PreTick()
            => Status = ModuleTickStatus.Alive;

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