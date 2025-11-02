using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    /// <summary>
    /// Represents a configuration section that specifies Harmony patches to be disabled.
    /// </summary>
    public class PatchSection
    {
        /// <summary>
        /// Gets or sets the collection of Harmony patch identifiers that are currently disabled.
        /// </summary>
        [Description("A list of disabled Harmony patches.")]
        public List<string> DisabledPatches { get; set; } = new List<string>();
    }
}