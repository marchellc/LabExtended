using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class PositionSynchronizerOptions
    {
        [Description("Whether or not to log position sync debug.")]
        public bool EnablePositionSyncDebug { get; set; } = true;

        [Description("Sets a custom position synchronization rate.")]
        public float PositionSyncRate { get; set; } = 0f;

        [Description("Rate of position synchronization debug messages.")]
        public float PositionDebugRate { get; set; } = 5f;
    }
}