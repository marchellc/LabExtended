using LabExtended.API.Containers;

using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class OtherSection
    {
        [Description("A collection of properties used to control behaviour specific to real players.")]
        public SwitchContainer PlayerToggles { get; set; } = SwitchContainer.DefaultPlayerSwitches;
        
        [Description("A collection of properties used to control behaviour specific to bot players.")]
        public SwitchContainer NpcToggles { get; set; } = SwitchContainer.DefaultNpcSwitches;
    }
}