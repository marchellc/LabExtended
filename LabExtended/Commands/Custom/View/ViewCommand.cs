using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.View;

/// <summary>
/// Base command for object descriptions.
/// </summary>
[Command("view", "Views the description of an object.")]
public partial class ViewCommand : CommandBase, IServerSideCommand { }