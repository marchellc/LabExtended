using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class ApiConfig
    {
        [Description("Whether or not to spawn an NPC as SCP-079 to manipulate camera rotation. May be required by some plugins.")]
        public bool EnableCameraNpc { get; set; } = true;

        [Description("Whether or not to show performance reports at the end of each round.")]
        public bool EnablePerformanceWatcher { get; set; } = true;

        [Description("Whether or not to log a message if a server's performance statistics per-round value decreases.")]
        public bool NotifyRoundOnMinChanged { get; set; } = true;

        [Description("Whether or not to log a message if a server's performance statistics per-round value increases.")]
        public bool NotifyRoundOnMaxChanged { get; set; } = true;

        [Description("Whether or not to log a message if a server's performance statistics overall value decreases.")]
        public bool NotifyOverallOnMinChanged { get; set; } = true;

        [Description("Whether or not to log a message if a server's performance statistics overall value increases.")]
        public bool NotifyOverallOnMaxChanged { get; set; } = true;

        [Description("Whether or not to show profiler statistics at round start.")]
        public bool EnableProfilerLogs { get; set; } = true;

        [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
        public bool DisableRoundLockOnLeave { get; set; } = true;

        [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
        public bool DisableLobbyLockOnLeave { get; set; } = true;

        [Description("A list of performance stats to not log.")]
        public List<string> NoLogPerformance { get; set; } = new List<string>() { "none" };

        [Description("A list of performance stats to log only once per round.")]
        public List<string> LogOncePerformance { get; set; } = new List<string>() { "none" };
    }
}