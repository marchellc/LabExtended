using LabExtended.Utilities;

namespace LabExtended.Core.Profiling
{
    /// <summary>
    /// Represents a frame recorded by the <see cref="ProfilerMarker"/>.
    /// </summary>
    public struct ProfilerFrame
    {
        /// <summary>
        /// Gets the frame's number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// Gets the frame's duration.
        /// </summary>
        public long Duration { get; }

        /// <summary>
        /// Gets the frame's duration (in milliseconds).
        /// </summary>
        public long Milliseconds => Duration / TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Gets the custom data passed by the profiler.
        /// </summary>
        public string Info { get; }

        internal ProfilerFrame(long duration, int number, string info)
        {
            Duration = duration;
            Number = number;
            Info = info;
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"[{Number}]: {Duration / TimeSpan.TicksPerMillisecond} ms{(!string.IsNullOrWhiteSpace(Info) ? $" (Comment: {Info})" : "")}";
    }
}