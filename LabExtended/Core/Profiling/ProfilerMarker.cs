using LabExtended.API.Collections.Locked;

using System.Diagnostics;

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
        public static ProfilerFrame TotalLongestFrame => AllMarkers.OrderBy(x => (x.WasEverInvoked ? x.LongestFrame.Duration : 0)).FirstOrDefault().LongestFrame;

        /// <summary>
        /// Gets the shortest frame in all markers.
        /// </summary>
        public static ProfilerFrame TotalShortestFrame => AllMarkers.OrderBy(x => (x.WasEverInvoked ? x.ShortestFrame.Duration : double.MaxValue)).FirstOrDefault().ShortestFrame;

        private bool _isInvoking = false;
        private bool _isFull = false;

        private int _samples = 0;

        private string _name;
        private string _invComm;

        private Stopwatch _stopwatch;
        private HashSet<ProfilerFrame> _frames;

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
            _samples = sampleCount;

            _frames = new HashSet<ProfilerFrame>(sampleCount > 0 ? sampleCount : 100);
            _stopwatch = new Stopwatch();

            _allMarkers.Add(this);
        }

        /// <summary>
        /// Gets the maximum frame duration.
        /// </summary>
        public TimeSpan MaxDuration => TimeSpan.FromTicks(_frames.Max(x => x.Duration));

        /// <summary>
        /// Gets the minimum frame duration.
        /// </summary>
        public TimeSpan MinDuration => TimeSpan.FromTicks(_frames.Min(x => x.Duration));

        /// <summary>
        /// Gets the average frame duration.
        /// </summary>
        public TimeSpan AvgDuration => !WasEverInvoked ? TimeSpan.Zero : TimeSpan.FromTicks((MaxDuration + MinDuration).Ticks / 2);

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
        public IReadOnlyCollection<ProfilerFrame> Frames => _frames;

        /// <summary>
        /// Gets a value indicating whether or not this marker has been marked.
        /// </summary>
        public bool WasEverInvoked => _frames.Count > 0;

        /// <summary>
        /// Gets a value indicating whether or not the marker is currently executing.
        /// </summary>
        public bool IsInvoking => _isInvoking;

        /// <summary>
        /// Gets a value indicating whether or not the marker is full.
        /// </summary>
        public bool IsFull => _isFull;

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
            if (_isFull)
                return;

            if (_samples > 0)
                _isFull = _frames.Count >= _samples;

            _isInvoking = true;
            _invComm = comment;

            _stopwatch.Start();
        }

        /// <summary>
        /// Marks the method's end.
        /// </summary>
        public void MarkEnd()
        {
            if (!_isInvoking || _isFull)
                return;

            _isInvoking = false;

            _stopwatch.Stop();
            _frames.Add(new ProfilerFrame(_stopwatch.ElapsedTicks, _frames.Count, _invComm));

            _invComm = null;
        }

        /// <summary>
        /// Clears all frames captured.
        /// </summary>
        public void Clear()
        {
            _frames.Clear();
            _isFull = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _allMarkers.Remove(this);

            _isInvoking = false;
            _isFull = false;

            _samples = 0;

            _frames.Clear();
            _frames = null;

            _stopwatch.Reset();
            _stopwatch = null;

            _invComm = null;
            _name = null;
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"{(!string.IsNullOrWhiteSpace(_name) ? _name : "Unnamed Marker")} (Avg: {AvgDuration.TotalMilliseconds} | Max: {MaxDuration.TotalMilliseconds} | Min: {MinDuration.TotalMilliseconds})";
    }
}