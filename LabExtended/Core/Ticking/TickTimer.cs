namespace LabExtended.Core.Ticking
{
    public class TickTimer
    {
        public float? DelayValue { get; set; } = 0f;

        public Tuple<float, float> DelayRange { get; set; }

        public bool IsProfiled { get; set; } = false;
        public bool IsFramed { get; set; } = false;

        private TickTimer() { }

        public static TickTimer GetFrames(int frameCount, bool useProfiler = false, bool useSeparate = false)
            => new TickTimer()
            {
                DelayValue = frameCount,

                IsProfiled = useProfiler,
                IsFramed = true
            };

        public static TickTimer GetStatic(float millisecondsDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickTimer()
            {
                DelayValue = millisecondsDelay,
                IsProfiled = useProfiler
            };

        public static TickTimer GetDynamic(float minDelay, float maxDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickTimer()
            {
                DelayRange = new Tuple<float, float>(minDelay, maxDelay),
                IsProfiled = useProfiler
            };

        public static TickTimer None => new TickTimer();
        public static TickTimer NoneProfiled => new TickTimer() { IsProfiled = true };
    }
}