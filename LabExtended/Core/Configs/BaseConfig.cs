using System.ComponentModel;

using LabExtended.Patches.Functions.Players;

namespace LabExtended.Core.Configs
{
    public class BaseConfig
    {
        [Description("Toggles logging of debug messages.")]
        public bool DebugEnabled { get; set; }
        
        [Description("Toggles debug logs of transpilers.")]
        public bool TranspilerDebugEnabled { get; set; }

        [Description("Toggles true color log formatting.")]
        public bool TrueColorEnabled { get; set; } = true;

        [Description("Whether or not to skip game version compatibility checks.")]
        public bool SkipGameCompatibility { get; set; }
        
        [Description("Whether or not to skip the client version compatibility checks.")]
        public bool SkipClientCompatibility { get; set; }

        [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
        public bool DisableRoundLockOnLeave { get; set; } = true;

        [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
        public bool DisableLobbyLockOnLeave { get; set; } = true;

        [Description("The maximum distance between a disarmer and a disarmed player before being automatically uncuffed.")]
        public float RemoveDisarmRange
        {
            get => DisarmValidateEntryPatch.DisarmDistance;
            set => DisarmValidateEntryPatch.DisarmDistance = value;
        }

        [Description("Sets a list of sources that cannot send debug messages.")]
        public List<string> DisabledDebugSources { get; set; } = new();
    }
}