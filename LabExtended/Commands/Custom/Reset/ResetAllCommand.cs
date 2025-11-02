using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Reset;

/// <summary>
/// Represents a server-side command that resets a specified property for all players.
/// </summary>
[Command("reset all", "Resets a property of all players.")]
public partial class ResetAllCommand : CommandBase, IServerSideCommand { }