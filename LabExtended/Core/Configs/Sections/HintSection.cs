using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class HintSection
    {
        [Description("The delay between each hint update (in milliseconds).")]
        public int UpdateInterval { get; set; } = 500;

        [Description("Duration of the displayed hint.")]
        public float HintDuration { get; set; } = 2.5f;
    }
}