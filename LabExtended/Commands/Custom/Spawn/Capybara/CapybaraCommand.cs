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
        [CommandParameter("Scale", "Scale of the capybara's model.")] Vector3 scale)
    {
        targetPlayer ??= Sender;

        var toy = new CapybaraToy()
        {
            Position = targetPlayer.Position,
            Rotation = targetPlayer.Rotation,
            
            Scale = scale
        };
        
        Ok($"Capybara spawned with ID {toy.NetId}");
    }
}