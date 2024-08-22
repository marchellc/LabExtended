using System.ComponentModel;
using LabExtended.Core.Configs.Api;

namespace LabExtended.Core.Configs
{
    /// <summary>
    /// Represents the loader's config.
    /// </summary>
    public class Config
    {
        [Description("Logging configuration.")]
        public LogOptions LogOptions { get; set; } = new LogOptions();

        [Description("Options for in-game round configuration.")]
        public RoundOptions RoundOptions { get; set; } = new RoundOptions();

        [Description("Voice chat configuration.")]
        public VoiceOptions VoiceOptions { get; set; } = new VoiceOptions();

        [Description("Advanced API configuration.")]
        public ApiOptions ApiOptions { get; set; } = new ApiOptions();

        public SwitchContainers SwitchContainers { get; set; } = new SwitchContainers();
    }
}