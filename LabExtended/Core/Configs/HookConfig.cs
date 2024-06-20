using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class HookConfig
    {
        [Description("Whether or not to pool method arguments when using the simple binder.")]
        public bool UsePoolingOnSimpleOverloads { get; set; } = true;

        [Description("Whether or not to pool method arguments when using the custom binder.")]
        public bool UsePoolingOnCustomOverloads { get; set; } = true;
    }
}