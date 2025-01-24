using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class HintSection
    {
        [Description("The delay between each hint update.")]
        public float UpdateInterval { get; set; } = 0.6f;

        [Description("Duration of the displayed hint.")]
        public float HintDuration { get; set; } = 2.5f;
    }
}