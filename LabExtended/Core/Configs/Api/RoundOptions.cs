using System.ComponentModel;

namespace LabExtended.Core.Configs.Api
{
    public class RoundOptions
    {
        [Description("Whether or not to disable Round Lock when the player who enabled it leaves.")]
        public bool DisableRoundLockOnLeave { get; set; } = true;

        [Description("Whether or not to disable Lobby Lock when the player who enabled it leaves.")]
        public bool DisableLobbyLockOnLeave { get; set; } = true;
    }
}