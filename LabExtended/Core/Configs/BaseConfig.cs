using System.ComponentModel;

namespace LabExtended.Core.Configs
{
    public class BaseConfig
    {
        [Description("Toggles logging of debug messages.")]
        public bool DebugEnabled { get; set; }

        [Description("Whether or not to skip game version compatibility checks.")]
        public bool SkipGameCompatibility { get; set; }

        [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
        public bool DisableRoundLockOnLeave { get; set; } = true;

        [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
        public bool DisableLobbyLockOnLeave { get; set; } = true;

        [Description("Sets a list of sources that cannot send debug messages.")]
        public List<string> DisabledDebugSources { get; set; } = new List<string>();
    }
}