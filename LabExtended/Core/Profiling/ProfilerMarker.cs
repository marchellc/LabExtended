using LabExtended.API.Collections.Locked;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Profiling
{
    /// <summary>
    /// Used to profile method timings.
    /// </summary>
    public class ProfilerMarker : IDisposable
    {
        internal static readonly LockedHashSet<ProfilerMarker> _allMarkers = new LockedHashSet<ProfilerMarker>();

        /// <summary>
        /// Gets all created markers.
        /// </summary>
        public static IReadOnlyList<ProfilerMarker> AllMarkers => _allMarkers;

        /// <summary>
        /// Gets the longest frame in all markers.
        /// </summary>
        public static ProfilerFrame TotalLongestFrame => AllMarkers.OrderBy(x => (x.WasEverInvoked ? x.LongestFrame.Duration.TotalMilliseconds : 0)).FirstOrDefault().LongestFrame;

        /// <summary>
        /// Gets the shortest frame in all markers.
        /// </summary>
        public static ProfilerFrame TotalShortestFrame => AllMarkers.OrderBy(x => (x.WasEverInvoked ? x.ShortestFrame.Duration.TotalMilliseconds : double.MaxValue)).FirstOrDefault().ShortestFrame;

        private DateTime _invStart;
        private DateTime _invEnd;

        private bool _isInvoking;
        private int _samples;

        private string _name;
        private string _invComm;

        private List<ProfilerFrame> _frames;

        /// <summary>
        /// Creates a new profiler.
        /// </summary>
        /// <param name="name">The name of the profiler (required).</param>
        /// <param name="sampleCount">The amount of samples to capture.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ProfilerMarker(string name, int sampleCount = -1)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            _name = name;
            _frames = new List<ProfilerFrame>();
            _samples = sampleCount;
            _allMarkers.Add(this);
        }

        /// <summary>
        /// Gets the time of the last marked start.
        /// </summary>
        public DateTime LastInvocation => _invEnd;

        /// <summary>
        /// Gets the maximum frame duration.
        /// </summary>
        public TimeSpan MaxDuration { get; private set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets the minimum frame duration.
        /// </summary>
        public TimeSpan MinDuration { get; private set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets the average frame duration.
        /// </summary>
        public TimeSpan AvgDuration => !WasEverInvoked ? TimeSpan.Zero : TimeSpan.FromTicks((MaxDuration + MinDuration).Ticks / 2);

        /// <summary>
        /// Gets the time passed since last start marking.
        /// </summary>
        public TimeSpan TimeSinceLastInvocation => !WasEverInvoked ? TimeSpan.Zero : DateTime.Now - LastInvocation;

        /// <summary>
        /// Gets the profiler's longest frame.
        /// </summary>
        public ProfilerFrame LongestFrame => _frames.OrderBy(f => f.Duration).Last();

        /// <summary>
        /// Gets the profiler's shortest frame.
        /// </summary>
        public ProfilerFrame ShortestFrame => _frames.OrderBy(f => f.Duration).First();

        /// <summary>
        /// Gets all captured frames.
        /// </summary>
        public IEnumerable<ProfilerFrame> Frames => _frames;

        /// <summary>
        /// Gets a value indicating whether or not this marker has been marked.
        /// </summary>
        public bool WasEverInvoked => _frames.Count > 0;

        /// <summary>
        /// Gets a value indicating whether or not the marker is currently executing.
        /// </summary>
        public bool IsInvoking => _isInvoking;

        /// <summary>
        /// Gets the marker's name.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Marks the method's start.
        /// </summary>
        /// <param name="comment">An optional comment used in <see cref="ProfilerFrame.Info"/>.</param>
        public void MarkStart(string comment = null)
        {
            if (_samples != -1 && _frames.Count >= _samples)
                return;

            _invStart = DateTime.Now;
            _isInvoking = true;
            _invComm = comment;
        }

        /// <summary>
        /// Marks the method's end.
        /// </summary>
        public void MarkEnd()
        {
            if (!_isInvoking)
                return;

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

        /// <summary>
        /// Logs the profiler's statistics to the console.
        /// </summary>
        /// <param name="isDebug">Whether or not to use the DEBUG tag instead of INFO.</param>
        public void LogStats(bool isDebug = false)
        {
            if (!WasEverInvoked)
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
                ApiLoader.Debug("Profiler", text);
            else
                ApiLoader.Info("Profiler", text);
        }

        /// <summary>
        /// Clears all frames captured.
        /// </summary>
        public void Clear()
            => _frames.Clear();

        /// <inheritdoc/>
        public void Dispose()
        {
            _allMarkers.Remove(this);

            _invEnd = DateTime.MinValue;
            _invStart = DateTime.MinValue;

            _isInvoking = false;

            _samples = 0;

            _frames.Clear();
            _frames = null;

            _invComm = null;
            _name = null;
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{(!string.IsNullOrWhiteSpace(_name) ? _name : "Unnamed Marker")} (Avg: {AvgDuration.TotalMilliseconds} | Max: {MaxDuration.TotalMilliseconds} | Min: {MinDuration.TotalMilliseconds})";

        /// <summary>
        /// Logs all marker's statistics to the console.
        /// </summary>
        /// <param name="isDebug">Whether or not to use the DEBUG tag instead of the INFO tag.</param>
        public static void LogAllMarkers(bool isDebug = false)
        {
            var builder = StringBuilderPool.Shared.Rent();

            builder.AppendLine($"Displaying statistics of '{_allMarkers.Count}' markers.");

            foreach (var marker in _allMarkers)
            {
                if (!marker.WasEverInvoked)
                    continue;

                var longest = marker.LongestFrame;
                var shortest = marker.ShortestFrame;

                builder.AppendLine($"&3{marker.Name}&r: &6{marker.AvgDuration.TotalMilliseconds} ms&r average, &6{longest.Duration.TotalMilliseconds} ms&r maximum{(!string.IsNullOrWhiteSpace(longest.Info) && longest.Info != "none" ? $" [{longest.Info}]" : "")}, &6{marker.MinDuration.TotalMilliseconds}&r minimum{(!string.IsNullOrWhiteSpace(shortest.Info) && shortest.Info != "none" ? $" [{shortest.Info}]" : "")}, (&6{marker._frames.Count} frames&r)");
            }

            if (isDebug)
                ApiLoader.Debug("Profiler", StringBuilderPool.Shared.ToStringReturn(builder));
            else
                ApiLoader.Info("Profiler", StringBuilderPool.Shared.ToStringReturn(builder));
        }

        /// <summary>
        /// Clears all marker's frames.
        /// </summary>
        public static void ClearAllMarkers()
        {
            foreach (var marker in _allMarkers)
                marker.Clear();
        }
    }
}