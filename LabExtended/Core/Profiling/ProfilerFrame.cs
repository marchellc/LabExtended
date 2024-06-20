namespace LabExtended.Core.Profiling
{
    public struct ProfilerFrame
    {
        public int Number { get; }

        public DateTime StartedAt { get; }
        public DateTime EndedAt { get; }

        public TimeSpan Duration { get; }

        public string Info { get; }

        public ProfilerFrame(DateTime startedAt, DateTime endedAt, TimeSpan duration, int number, string info)
        {
            StartedAt = startedAt;
            EndedAt = endedAt;
            Duration = duration;
            Number = number;
            Info = info;
        }

        public override string ToString()
            => $"[{Number}]: {StartedAt} - {EndedAt} ({Duration.TotalMilliseconds} ms){(!string.IsNullOrWhiteSpace(Info) ? $" (Comment: {Info})" : "")}";
    }
}