using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class TickSection
    {
        [Description("Whether or not to use player loops for Unity tick distribution.")]
        public bool UseTickLoop { get; set; } = true;

        [Description("Whether or not to use a custom Unity component for tick distribution.")]
        public bool UseTickComponent { get; set; } = false;

        [Description("Whether or not to use a custom MEC Coroutine for tick distribution.")]
        public bool UseTickCoroutine { get; set; } = false;

        [Description("Whether or not to enable distributor performance metrics.")]
        public bool EnableMetrics { get; set; } = true;
    }
}