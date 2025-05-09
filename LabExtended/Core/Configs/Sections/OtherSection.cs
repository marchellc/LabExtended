using LabExtended.API.Containers;

using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class OtherSection
    {
        [Description("Whether or not to replace Mirror's network player loop.")]
        public bool MirrorAsync { get; set; }
        
        [Description("Whether or not to enable custom SCP-914 recipes. This is a feature in testing and may break a lot of things.")]
        public bool Scp914CustomRecipes { get; set; }
        
        [Description("A collection of properties used to control behaviour specific to real players.")]
        public SwitchContainer PlayerToggles { get; set; } = SwitchContainer.DefaultPlayerSwitches;
        
        [Description("A collection of properties used to control behaviour specific to bot players.")]
        public SwitchContainer NpcToggles { get; set; } = SwitchContainer.DefaultNpcSwitches;
    }
}