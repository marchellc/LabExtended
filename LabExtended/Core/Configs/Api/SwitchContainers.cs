using System.ComponentModel;
using LabExtended.API.Containers;
using LabExtended.API.Npcs;

namespace LabExtended.Core.Configs.Api {
    public class SwitchContainers {
        [Description("Player Switches for regular players")]
        public SwitchContainer PlayerSwitches { get; set; } = new SwitchContainer();

        [Description("Player Switches for regular players")]
        public SwitchContainer NpcSwitches { get; set; } = NpcHandler.DefaultNpcSwitches;
    }
}
