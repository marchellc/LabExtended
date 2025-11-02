using LabExtended.Commands.Interfaces;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.Spawn;

/// <summary>
/// Represents a command that spawns multiple entities or objects on the server.
/// </summary>
/// <remarks>This command is intended for server-side use and should be executed in contexts where spawning
/// multiple items or entities is required. The specific behavior and available options depend on the implementation in
/// derived classes or command handlers.</remarks>
[Attributes.Command("spawn", "Spawns multiple things.")]
public partial class SpawnCommand : CommandBase, IServerSideCommand { }