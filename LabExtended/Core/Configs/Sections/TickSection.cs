using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking;

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

        [Description("A dictionary of custom ticks. You can set which method will be registered to which tick distributor.")]
        public Dictionary<string, string> CustomTicks { get; set; } = new Dictionary<string, string>()
        {
            ["TeslaGates"] = "UnityTickDistributor",

            ["CustomItems"] = "UnityTickDistributor",
            ["CustomUsables"] = "UnityTickDistributor",

            ["TransientModules"] = "UnityTickDistributor",
            ["ThreadedVoiceOutput"] = "UnityTickDistributor",

            ["PositionSync"] = "UnityTickDistributor",
            ["RoleSync"] = "UnityTickDistributor",

            ["ThreadSafe"] = "UnityTickDistributor"
        };

        public ITickDistributor GetCustomOrDefault(string tickName, ITickDistributor defaultDistributor)
        {
            if (CustomTicks.TryGetValue(tickName, out var distributorName))
                return TickDistribution.GetDistributor(distributorName);

            return defaultDistributor;
        }
    }
}