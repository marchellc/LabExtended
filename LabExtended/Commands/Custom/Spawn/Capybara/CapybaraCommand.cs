using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    [CommandOverload("capybara", "Spawns a capybara.")]
    public void CapybaraOverload(
        [CommandParameter("Target", "The target player.")] ExPlayer targetPlayer, 
        [CommandParameter("Scale", "Scale of the capybara's model.")] Vector3? scale = null)
    {
        targetPlayer ??= Sender;
        
        var toy = new CapybaraToy(targetPlayer.Position, targetPlayer.Rotation)
        {
            Scale = scale ?? Vector3.one
        };
        
        Ok($"Capybara spawned with ID {toy.NetId}");
    }
}