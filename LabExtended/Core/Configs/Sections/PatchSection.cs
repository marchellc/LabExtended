using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class PatchSection
    {
        [Description("A list of disabled Harmony patches.")]
        public List<string> DisabledPatches { get; set; } = new List<string>();
    }
}