using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class ApiConfig
    {
        [Description("Whether or not to spawn an NPC as SCP-079 to manipulate camera rotation. May be required by some plugins.")]
        public bool EnableCameraNpc { get; set; } = true;

        [Description("Whether or not to show performance reports at the end of each round.")]
        public bool EnablePerformanceWatcher { get; set; } = true;

        [Description("Whether or not to show profiler statistics at round start.")]
        public bool EnableProfilerLogs { get; set; } = true;

        [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
        public bool DisableRoundLockOnLeave { get; set; } = true;

        [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
        public bool DisableLobbyLockOnLeave { get; set; } = true;

        [Description("Whether or not to log position sync debug.")]
        public bool EnablePositionSyncDebug { get; set; } = true;

        [Description("Sets a custom position synchronization rate.")]
        public float PositionSyncRate { get; set; } = 0f;

        [Description("Rate of position synchronization debug messages.")]
        public float PositionDebugRate { get; set; } = 5f;

        [Description("A list of performance stats to not log.")]
        public List<string> NoLogPerformance { get; set; } = new List<string>() { "none" };
    }
}