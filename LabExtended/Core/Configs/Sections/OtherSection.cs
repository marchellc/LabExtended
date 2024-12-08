using LabExtended.API.Containers;

using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    public class OtherSection
    {
        [Description("A collection of properties used to control behaviour specific to real players.")]
        public SwitchContainer Players { get; set; } = new SwitchContainer();
    }
}