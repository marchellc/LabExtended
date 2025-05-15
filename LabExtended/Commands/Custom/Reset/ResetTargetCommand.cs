using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

[Command("reset target", "Resets a property of a given target object.")]
public partial class ResetTargetCommand : CommandBase, IServerSideCommand { }