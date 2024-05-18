using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class HookConfig
    {
        [Description("Sets the maximum amount of time (in milliseconds) the API will wait for a coroutine to execute.")]
        public float CoroutineTimeout { get; set; } = 1000f;

        [Description("Whether or not to completely disable all NW API events (doesn't apply to events specified in DisableWhitelist!).")]
        public bool DisableNwEvents { get; set; } = true;

        [Description("A list of event names to not disable.")]
        public List<string> DisableWhitelist { get; set; } = new List<string>();
    }
}