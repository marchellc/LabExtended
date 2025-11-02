using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

/// <summary>
/// Represents a command that resets a property of a specified target object.
/// </summary>
[Command("reset target", "Resets a property of a given target object.")]
public partial class ResetTargetCommand : CommandBase, IServerSideCommand { }