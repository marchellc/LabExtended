using LabExtended.Core.Configs.Sections;

using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    /// <summary>
    /// Contains API-related configuration.
    /// </summary>
    public class ApiConfig
    {
        [Description("Voice chat configuration.")]
        public VoiceSection VoiceSection { get; set; } = new();
        
        [Description("Command System configuration.")]
        public CommandSection CommandSection { get; set; } = new();

        [Description("Hint system configuration.")]
        public HintSection HintSection { get; set; } = new();

        [Description("Player data synchronization configuration.")]
        public SynchronizationSection SynchronizationSection { get; set; } = new();

        [Description("Patching options.")] 
        public PatchSection PatchSection { get; set; } = new();

        [Description("Unity Engine Player Loop configuration.")]
        public LoopSection LoopSection { get; set; } = new();

        [Description("Configuration for other things.")]
        public OtherSection OtherSection { get; set; } = new();
    }
}