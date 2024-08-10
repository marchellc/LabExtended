using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class PerformanceOptions
    {
        [Description("Whether or not to show performance reports at the end of each round.")]
        public bool EnablePerformanceWatcher { get; set; } = true;

        [Description("Whether or not to show profiler statistics at round start.")]
        public bool EnableProfilerLogs { get; set; } = true;

        [Description("A list of performance stats to not log.")]
        public List<string> NoLogPerformance { get; set; } = new List<string>() { "none" };
    }
}