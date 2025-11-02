using LabExtended.Core.Configs.Sections;

using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    /// <summary>
    /// Contains API-related configuration.
    /// </summary>
    public class ApiConfig
    {
        /// <summary>
        /// Gets or sets the configuration settings for file storage.
        /// </summary>
        [Description("File Storage configuration.")]
        public StorageSection StorageSection { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the configuration settings for voice chat.
        /// </summary>
        [Description("Voice chat configuration.")]
        public VoiceSection VoiceSection { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the configuration settings for the command system.
        /// </summary>
        [Description("Command System configuration.")]
        public CommandSection CommandSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the configuration settings for the hint system.
        /// </summary>
        [Description("Hint system configuration.")]
        public HintSection HintSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the patching options to be applied.
        /// </summary>
        [Description("Patching options.")] 
        public PatchSection PatchSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the Unity Engine player loop configuration.
        /// </summary>
        [Description("Unity Engine Player Loop configuration.")]
        public LoopSection LoopSection { get; set; } = new();

        /// <summary>
        /// Gets or sets the configuration settings for additional application features.
        /// </summary>
        [Description("Configuration for other things.")]
        public OtherSection OtherSection { get; set; } = new();
    }
}