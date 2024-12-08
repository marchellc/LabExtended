using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class VoiceSection
    {
        [Description("Disables the custom voice chat system if set to true.")]
        public bool DisableCustomVoice { get; set; }

        [Description("Sets a custom voice chat rate limit.")]
        public int CustomRateLimit { get; set; } = 128;
    }
}