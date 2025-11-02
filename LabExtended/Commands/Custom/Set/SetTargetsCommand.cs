using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

/// <summary>
/// Represents a command that sets a property on a collection of target objects.
/// </summary>
/// <remarks>Use this command to update a specific property across multiple target objects in a single operation.
/// The command is typically used in scenarios where batch updates are required for consistency or efficiency.</remarks>
[Command("set targets", "Sets a property on a list of target objects.")]
public partial class SetTargetsCommand : CommandBase, IServerSideCommand { }