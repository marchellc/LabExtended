using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class SynchronizationSection
    {
        [Description("Sets a custom position synchronization rate.")]
        public float PositionSyncRate { get; set; } = 0f;
    }
}
