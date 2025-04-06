using LabExtended.Commands.Interfaces;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.Spawn;

[Attributes.Command("spawn", "Spawns multiple things.")]
public partial class SpawnCommand : CommandBase, IServerSideCommand { }