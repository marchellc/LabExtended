using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class LogConfig
    {
        [Description("Toggles logging of debug messages.")]
        public bool DebugEnabled { get; set; }

        [Description("Toggles logging from the support library.")]
        public bool PipeEnabled { get; set; } = true;

        [Description("Sets a list of sources that cannot send debug messages.")]
        public List<string> DisabledSources { get; set; } = new List<string>();

        [Description("Sets a list of sources that will always send debug messages.")]
        public List<string> EnabledSources { get; set; } = new List<string>();
    }
}