using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class VoiceOptions
    {
        [Description("Sets a custom voice chat rate limit.")]
        public int CustomRateLimit { get; set; } = 128;

        [Description("Disables the custom voice chat system if set to true.")]
        public bool DisableCustomVoice { get; set; }

        [Description("Options for voice chat threading.")]
        public ThreadedVoiceOptions ThreadedVoiceOptions { get; set; } = new ThreadedVoiceOptions();
    }
}