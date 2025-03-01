using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class VoiceSection
    {
        [Description("Disables the threaded voice chat system if set to true.")]
        public bool DisableThreadedVoice { get; set; }

        [Description("Sets a custom voice chat rate limit.")]
        public int CustomRateLimit { get; set; } = 128;
    }
}