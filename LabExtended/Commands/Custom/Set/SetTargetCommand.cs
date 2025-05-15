using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

[Command("set target", "Sets a property on a target object.")]
public partial class SetTargetCommand : CommandBase, IServerSideCommand { }