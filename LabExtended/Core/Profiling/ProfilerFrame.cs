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
        /// Gets the time of the frame's start.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// Gets the time of the frame's end.
        /// </summary>
        public DateTime EndedAt { get; }

        /// <summary>
        /// Gets the frame's duration.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the custom data passed by the profiler.
        /// </summary>
        public string Info { get; }

        internal ProfilerFrame(DateTime startedAt, DateTime endedAt, TimeSpan duration, int number, string info)
        {
            StartedAt = startedAt;
            EndedAt = endedAt;
            Duration = duration;
            Number = number;
            Info = info;
        }

        /// <inheritdoc/>
        public override string ToString()
            => $"[{Number}]: {StartedAt} - {EndedAt} ({Duration.TotalMilliseconds} ms){(!string.IsNullOrWhiteSpace(Info) ? $" (Comment: {Info})" : "")}";
    }
}