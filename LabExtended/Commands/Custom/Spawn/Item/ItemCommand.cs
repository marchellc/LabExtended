using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    /// <summary>
    /// Spawns the specified number of items of a given type at the target player's position.
    /// </summary>
    /// <param name="target">The player at whose position the items will be spawned. Cannot be null.</param>
    /// <param name="amount">The number of items to spawn. Must be greater than zero.</param>
    /// <param name="type">The type of item to spawn.</param>
    /// <param name="scale">The scale to apply to each spawned item. If null, a scale of one is used.</param>
    [CommandOverload("item", "Spawns an item.", null)]
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