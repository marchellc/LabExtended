using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

[Command("set targets", "Sets a property on a list of target objects.")]
public partial class SetTargetsCommand : CommandBase, IServerSideCommand { }