using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class LoopSection
    {
        [Description("Whether or not to modify Player loops (adding a custom one and removing ones specified in the config below).")]
        public bool ModifyLoops { get; set; } = false;

        [Description("A list of player loops that will be removed.")]
        public List<string> RemovedLoops { get; set; } = new List<string>();

        [Description("A list of all present loops.")]
        public List<string> AllLoops { get; set; } = new List<string>();
    }
}
