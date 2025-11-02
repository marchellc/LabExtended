using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

/// <summary>
/// Represents a command that sets a specified property on all target objects.
/// </summary>
/// <remarks>Use this command to apply a property value across all objects managed by the system. The specific
/// property and value to set are typically provided as command parameters. This command is intended for scenarios where
/// bulk updates are required.</remarks>
[Command("set all", "Sets a property on all objects.")]
public partial class SetAllCommand : CommandBase, IServerSideCommand { }