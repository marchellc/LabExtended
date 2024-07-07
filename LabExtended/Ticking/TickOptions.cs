namespace LabExtended.Ticking
{
    public class TickOptions
    {
        public TickDelayType DelayType { get; set; } = TickDelayType.None;
        public float DelayValue { get; set; } = 0f;

        public Tuple<float, float> DelayRange { get; set; }

        public bool IsProfiled { get; set; } = false;

        private TickOptions() { }

        public static TickOptions GetFrames(int frameCount, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Frames,
                DelayValue = frameCount,

                IsProfiled = useProfiler
            };

        public static TickOptions GetStatic(float millisecondsDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Static,
                DelayValue = millisecondsDelay,

                IsProfiled = useProfiler
            };

        public static TickOptions GetDynamic(float minDelay, float maxDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Dynamic,
                DelayRange = new Tuple<float, float>(minDelay, maxDelay),

                IsProfiled = useProfiler
            };

        public static TickOptions None => new TickOptions() { DelayType = TickDelayType.None };
        public static TickOptions NoneProfiled => new TickOptions() { DelayType = TickDelayType.None, IsProfiled = true };
    }
}