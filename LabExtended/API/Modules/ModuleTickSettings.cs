namespace LabExtended.API.Modules
{
    public struct ModuleTickSettings
    {
        public readonly float? MinTime;
        public readonly float? MaxTime;
        public readonly float? FixedTime;

        public readonly int? FrameCount;

        public readonly ModuleTickType TickType;

        public ModuleTickSettings(float fixedTime)
        {
            MinTime = null;
            MaxTime = null;
            FrameCount = null;

            FixedTime = fixedTime;

            TickType = ModuleTickType.Fixed;
        }

        public ModuleTickSettings(float minTime, float maxTime)
        {
            MinTime = minTime;
            MaxTime = maxTime;

            TickType = ModuleTickType.Randomized;

            FrameCount = null;
            FixedTime = null;
        }

        public ModuleTickSettings(int frameCount)
        {
            FrameCount = frameCount;

            TickType = ModuleTickType.OnUpdate;

            MinTime = null;
            MaxTime = null;
            FixedTime = null;
        }

        public ModuleTickSettings(ModuleTickType tickType, float? minTime, float? maxTime, float? fixedTime, int? frameCount)
        {
            TickType = tickType;
            MinTime = minTime;
            MaxTime = maxTime;
            FixedTime = fixedTime;
            FrameCount = frameCount;
        }
    }
}