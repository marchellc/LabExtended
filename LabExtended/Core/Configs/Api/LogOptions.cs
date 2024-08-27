using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class LogOptions
    {
        [Description("Toggles logging of debug messages.")]
        public bool DebugEnabled { get; set; }

        [Description("Sets a list of sources that cannot send debug messages.")]
        public List<string> DisabledSources { get; set; } = new List<string>();

        [Description("Sets a list of sources that will always send debug messages.")]
        public List<string> EnabledSources { get; set; } = new List<string>();
    }
}