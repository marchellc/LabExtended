using System.ComponentModel;

using LabExtended.Core.Configs;

namespace LabExtended.Core
{
    public class ExLoaderConfig
    {
        [Description("Logging configuration.")]
        public LogConfig Logging { get; set; } = new LogConfig();

        [Description("Hook configuration.")]
        public HookConfig Hooks { get; set; } = new HookConfig();
    }
}