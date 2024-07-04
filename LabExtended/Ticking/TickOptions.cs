using LabExtended.Core;
using LabExtended.Core.Profiling;

namespace LabExtended.Ticking
{
    public class TickOptions
    {
        internal string _tickId;

        internal DateTime? _nextTickTime;
        internal DateTime? _lastTickTime;

        internal int _passedFrames = 0;
        internal bool _isPaused = false;

        internal ProfilerMarker _marker;

        public TickDelayType DelayType { get; set; } = TickDelayType.None;
        public float DelayValue { get; set; } = 0f;

        public Tuple<float, float> DelayRange { get; set; }

        public bool IsProfiled { get; set; } = false;
        public bool IsSeparate { get; set; } = false;

        public bool CanTick
        {
            get
            {
                if (_isPaused)
                    return false;

                if (DelayType is TickDelayType.None)
                    return true;

                if (_nextTickTime.HasValue && DateTime.Now < _nextTickTime.Value)
                    return false;

                if (DelayType is TickDelayType.Frames && ++_passedFrames < DelayValue)
                    return false;

                return true;
            }
        }

        private TickOptions() { }

        internal void RegisterTickStart()
        {
            if (IsProfiled)
                _marker.MarkStart();
        }

        internal void RegisterTickEnd()
        {
            if (IsProfiled)
                _marker.MarkEnd();

            _passedFrames = 0;

            if (DelayType is TickDelayType.Static && DelayValue > 0f)
                _nextTickTime = DateTime.Now.AddMilliseconds(DelayValue);
            else if (DelayType is TickDelayType.Dynamic && DelayRange != null && DelayRange.Item2 > DelayRange.Item1)
                _nextTickTime = DateTime.Now.AddMilliseconds(UnityEngine.Random.Range(DelayRange.Item1, DelayRange.Item2));
            else
                _nextTickTime = null;
        }

        public static TickOptions GetFrames(int frameCount, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Frames,
                DelayValue = frameCount,

                IsProfiled = useProfiler,
                IsSeparate = useSeparate
            };

        public static TickOptions GetStatic(float millisecondsDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Static,
                DelayValue = millisecondsDelay,

                IsProfiled = useProfiler,
                IsSeparate = useSeparate
            };

        public static TickOptions GetDynamic(float minDelay, float maxDelay, bool useProfiler = false, bool useSeparate = false)
            => new TickOptions()
            {
                DelayType = TickDelayType.Dynamic,
                DelayRange = new Tuple<float, float>(minDelay, maxDelay),

                IsProfiled = useProfiler,
                IsSeparate = useSeparate
            };

        public static TickOptions None => new TickOptions() { DelayType = TickDelayType.None };
        public static TickOptions NoneProfiled => new TickOptions() { DelayType = TickDelayType.None, IsProfiled = true };

        public static TickOptions NoneSeparate => new TickOptions() { DelayType = TickDelayType.None, IsSeparate = true };
        public static TickOptions NoneSeparateProfiled => new TickOptions() { DelayType = TickDelayType.None, IsSeparate = true, IsProfiled = true };
    }
}