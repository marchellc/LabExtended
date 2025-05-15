using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

[Command("set all", "Sets a property on all objects.")]
public partial class SetAllCommand : CommandBase, IServerSideCommand { }