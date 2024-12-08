using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class MapSection
    {
        [Description("The interval between each Tesla Gate tick.")]
        public int TeslaGateTickRate { get; set; } = 100;
    }
}
