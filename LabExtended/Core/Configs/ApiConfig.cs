using LabExtended.Core.Configs.Sections;

using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class ApiConfig
    {
        [Description("Map-specific configuration.")]
        public MapSection MapSection { get; set; } = new MapSection();

        [Description("Voice chat configuration.")]
        public VoiceSection VoiceSection { get; set; } = new VoiceSection();

        [Description("Hint system configuration.")]
        public HintSection HintSection { get; set; } = new HintSection();

        [Description("Pooling API configuration.")]
        public PoolSection PoolSection { get; set; } = new PoolSection();

        [Description("Multi Threading configuration.")]
        public MultiThreadSection MultiThreadSection { get; set; } = new MultiThreadSection();
        
        [Description("Voice chat threading configuration.")]
        public ThreadedVoiceSection ThreadedVoiceSection { get; set; } = new ThreadedVoiceSection();

        [Description("Player data synchronization configuration.")]
        public SynchronizationSection SynchronizationSection { get; set; } = new SynchronizationSection();

        [Description("Patching options.")]
        public PatchSection PatchSection { get; set; } = new PatchSection();

        [Description("Unity Engine Player Loop configuration.")]
        public LoopSection LoopSection { get; set; } = new LoopSection();

        [Description("Configuration for other things.")]
        public OtherSection OtherSection { get; set; } = new OtherSection();
    }
}