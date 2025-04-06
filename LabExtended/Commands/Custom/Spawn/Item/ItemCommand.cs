using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    [CommandOverload("item", "Spawns an item.")]
    public void ItemOverload(
        [CommandParameter("Target", "The target player.")] ExPlayer target,
        [CommandParameter("Amount", "The amount of items to spawn.")] int amount, 
        [CommandParameter("Type", "The type of item to spawn.")] ItemType type,
        [CommandParameter("Scale", "The scale of each item (defaults to one).")] Vector3? scale = null)
    {
        scale ??= Vector3.one;
        
        var items = ExMap.SpawnItems<ItemPickupBase>(type, amount, target.Position, scale.Value, target.Rotation);
        
        Ok($"Spawned {items.Count} pickup(s) of {type}: {string.Join(", ", items.Select(p => p.netId))}");
    }
}