using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

/// <summary>
/// Represents a command that resets a specified property on a collection of target objects.
/// </summary>
/// <remarks>Use this command to revert one or more properties of target objects to their default or initial
/// state. The specific property and targets to reset are determined by the command's parameters. This command is
/// typically used in scenarios where object state needs to be restored or cleared in bulk.</remarks>
[Command("reset targets", "Resets a property of a given list of target objects.")]
public partial class ResetTargetsCommand : CommandBase, IServerSideCommand { }