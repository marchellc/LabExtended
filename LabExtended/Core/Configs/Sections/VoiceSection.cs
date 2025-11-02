using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    /// <summary>
    /// Represents configuration options for the voice chat system.
    /// </summary>
    public class VoiceSection
    {
        /// <summary>
        /// Gets or sets a value indicating whether the threaded voice chat system is disabled.
        /// </summary>
        [Description("Disables the threaded voice chat system if set to true.")]
        public bool DisableThreadedVoice { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the base game's OnVoiceMessageReceiving event is invoked.
        /// </summary>
        /// <remarks>Set this property to <see langword="true"/> to enable compatibility with systems that
        /// rely on the legacy event. Disabling it may improve performance or prevent duplicate event handling if the
        /// legacy event is not required.</remarks>
        [Description("Whether or not to call the base game's OnVoiceMessageReceiving event.")]
        public bool EnableLegacyEvent { get; set; }

        /// <summary>
        /// Gets or sets the custom rate limit for voice chat operations.
        /// </summary>
        /// <remarks>The rate limit determines the maximum number of voice chat requests allowed per
        /// second. Adjust this value to control bandwidth usage or to comply with server requirements.</remarks>
        [Description("Sets a custom voice chat rate limit.")]
        public int CustomRateLimit { get; set; } = 128;

        /// <summary>
        /// Gets or sets the maximum number of modified voice packets that can be processed per frame.
        /// </summary>
        [Description("Sets the maximum amount of modified voice packets that can be processed per frame.")]
        public int MaxThreadOutput { get; set; } = 100;
    }
}