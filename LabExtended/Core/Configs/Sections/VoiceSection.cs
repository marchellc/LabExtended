using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class VoiceSection
    {
        [Description("Disables the threaded voice chat system if set to true.")]
        public bool DisableThreadedVoice { get; set; }
        
        [Description("Whether or not to call the base game's OnVoiceMessageReceiving event.")]
        public bool EnableLegacyEvent { get; set; }

        [Description("Sets a custom voice chat rate limit.")]
        public int CustomRateLimit { get; set; } = 128;

        [Description("Sets the maximum amount of modified voice packets that can be processed per frame.")]
        public int MaxThreadOutput { get; set; } = 100;
    }
}