using Common.IO.Collections;

namespace LabExtended.Core.Profiling
{
    public class ProfilerMarker
    {
        internal static readonly LockedList<ProfilerMarker> _allMarkers = new LockedList<ProfilerMarker>();

        public static IEnumerable<ProfilerMarker> AllMarkers => _allMarkers;

        private DateTime _invStart;
        private DateTime _invEnd;

        private string _invComm;

        private bool _isInvoking;

        private readonly int _samples;
        private readonly string _name;
        private readonly List<ProfilerFrame> _frames;

        public ProfilerMarker(string name, int sampleCount = -1)
        {
            _name = name ?? "Unnamed Marker";
            _frames = new List<ProfilerFrame>();
            _samples = sampleCount;
            _allMarkers.Add(this);
        }

        public DateTime LastInvocation => _invEnd;

        public TimeSpan MaxDuration { get; private set; } = TimeSpan.Zero;
        public TimeSpan MinDuration { get; private set; } = TimeSpan.Zero;

        public TimeSpan AvgDuration => !WasEverInvoked ? TimeSpan.Zero : TimeSpan.FromTicks((MaxDuration + MinDuration).Ticks / 2);
        public TimeSpan TimeSinceLastInvocation => !WasEverInvoked ? TimeSpan.Zero : DateTime.Now - LastInvocation;

        public IEnumerable<ProfilerFrame> Frames => _frames;

        public bool WasEverInvoked { get; private set; }
        public bool IsInvoking => _isInvoking;

        public string Name => _name;

        public void MarkStart(string comment = null)
        {
            if (_samples != -1 && _frames.Count >= _samples)
                return;

            _invStart = DateTime.Now;
            _isInvoking = true;
            _invComm = comment;
        }

        public void MarkEnd()
        {
            if (!_isInvoking)
                return;

            WasEverInvoked = true;

            _invEnd = DateTime.Now;
            _isInvoking = false;

            var time = _invEnd - _invStart;
            var frame = new ProfilerFrame(_invStart, _invEnd, time, _frames.Count, _invComm);

            if (MaxDuration == TimeSpan.Zero || frame.Duration > MaxDuration)
                MaxDuration = frame.Duration;

            if (MinDuration == TimeSpan.Zero || frame.Duration < MinDuration)
                MinDuration = frame.Duration;

            _frames.Add(frame);
            _invComm = null;
        }

        public void LogStats(bool isDebug = false)
        {
            if (!WasEverInvoked || _frames.Count < 1)
                return;

            var longestFrame = _frames.OrderBy(f => f.Duration).Last();
            var shortestFrame = _frames.OrderBy(f => f.Duration).First();

            var text = $"\nProfiling marker - &3{_name}&r\n" +
                $">- &3Executed&r &6{_frames.Count}&r times &3with an average duration of&r &6{AvgDuration.TotalMilliseconds} ms&r\n" +
                $">- &3Longest frame:&r\n" +
                $" &3-> Number:&r &6{longestFrame.Number}&r\n" +
                $" &3-> Duration:&r &6{longestFrame.Duration.TotalMilliseconds} ms&r\n" +
                $" &3-> Comment:&r &6{longestFrame.Info ?? "none"}&r\n" +
                $">- &3Shortest frame:&r\n" +
                $" &3-> Number:&r &6{shortestFrame.Number}&r\n" +
                $" &3-> Duration:&r &6{shortestFrame.Duration.TotalMilliseconds} ms&r\n" +
                $" &3-> Comment:&r &6{shortestFrame.Info ?? "none"}&r";

            if (isDebug)
                ExLoader.Debug("Profiling", text);
            else
                ExLoader.Info("Profiling", text);
        }

        public void Clear()
            => _frames.Clear();

        public override string ToString()
            => $"{(!string.IsNullOrWhiteSpace(_name) ? _name : "Unnamed Marker")} (Avg: {AvgDuration.TotalMilliseconds} | Max: {MaxDuration.TotalMilliseconds} | Min: {MinDuration.TotalMilliseconds})";

        public static void LogAllMarkers(bool isDebug = false)
        {
            foreach (var marker in _allMarkers)
                marker.LogStats(isDebug);
        }

        public static void ClearAllMarkers()
        {
            foreach (var marker in _allMarkers)
                marker.Clear();
        }
    }
}