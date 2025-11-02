using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Set;

/// <summary>
/// Represents a command that sets a property on a target object.
/// </summary>
/// <remarks>Use this command to modify properties of a specified target object on the server. This command is
/// typically used in scenarios where dynamic configuration or state changes are required at runtime. The specific
/// property to set and its value are determined by the command's parameters.</remarks>
[Command("set target", "Sets a property on a target object.")]
public partial class SetTargetCommand : CommandBase, IServerSideCommand { }