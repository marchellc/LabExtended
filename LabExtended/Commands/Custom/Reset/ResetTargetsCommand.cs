using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

[Command("reset targets", "Resets a property of a given list of target objects.")]
public partial class ResetTargetsCommand : CommandBase, IServerSideCommand { }