using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class ApiOptions
    {
        [Description("Whether or not to spawn an NPC as SCP-079 to manipulate camera rotation. May be required by some plugins.")]
        public bool EnableCameraNpc { get; set; } = true;

        [Description("Options for the custom position synchronizer.")]
        public PositionSynchronizerOptions PositionSynchronizerOptions { get; set; } = new PositionSynchronizerOptions();

        [Description("Options for performance monitoring.")]
        public PerformanceOptions PerformanceOptions { get; set; } = new PerformanceOptions();
    }
}