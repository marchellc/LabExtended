using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class ThreadedVoiceOptions
    {
        [Description("Whether or not to enable voice chat threading. This is an experimental feature that may not work.")]
        public bool IsEnabled { get; set; }
    }
}