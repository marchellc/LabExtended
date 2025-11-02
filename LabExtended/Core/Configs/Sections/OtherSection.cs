using LabExtended.API.Containers;

using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    /// <summary>
    /// Represents a configuration section that defines toggleable options for both real and bot players.
    /// </summary>
    public class OtherSection
    {
        /// <summary>
        /// Gets or sets the collection of switches that control behavior specific to real players.
        /// </summary>
        [Description("A collection of properties used to control behaviour specific to real players.")]
        public SwitchContainer PlayerToggles { get; set; } = SwitchContainer.DefaultPlayerSwitches;
        
        /// <summary>
        /// Gets or sets the collection of switches that control behavior specific to non-player character (NPC) bots.
        /// </summary>
        [Description("A collection of properties used to control behaviour specific to bot players.")]
        public SwitchContainer NpcToggles { get; set; } = SwitchContainer.DefaultNpcSwitches;
    }
}