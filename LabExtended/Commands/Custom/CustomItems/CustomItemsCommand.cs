using InventorySystem;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using MapGeneration;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.Commands.Custom.CustomItems;

[Command("customitems", "Custom Items management commands.", "citems")]
public class CustomItemsCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("give", "Gives a Custom Item to a specific player.")]
    public void GiveCommand(
        [CommandParameter("ID", "ID of the Custom Item")] ushort itemId, 
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        if (!CustomItemRegistry.TryGetHandler(itemId, out var handler))
        {
            Fail($"Unknown Custom Item: {itemId}");
            return;
        }

        var player = target ?? Sender;

        if (player.Inventory.ItemCount >= 8)
        {
            Fail("The target player's inventory is full.");
            return;
        }

        var item = handler.Give(player);

        Ok($"Added item \"{handler.Name}\" to player \"{player.Nickname}\" ({player.ClearUserId}), serial: {item.Item.ItemSerial}");
    }

    [CommandOverload("spawnplayer", "Spawns a Custom Item at a specific player.")]
    public void SpawnPlayerCommand(
        [CommandParameter("ID", "ID of the Custom Item.")] ushort itemId,
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        if (!CustomItemRegistry.TryGetHandler(itemId, out var handler))
        {
            Fail($"Unknown Custom Item: {itemId}");
            return;
        }

        var player = target ?? Sender;

        if (!player.Role.IsAlive)
        {
            Fail($"Player \"{player.Nickname}\" ({player.UserId}) is not alive.");
            return;
        }

        var item = handler.Spawn(player.Position, player.Rotation, player);
        
        Ok($"Spawned item \"{handler.Name}\" at player \"{player.Nickname}\" ({player.ClearUserId}), serial: {item.Pickup.Info.Serial}");
    }

    [CommandOverload("spawnpos", "Spawns a Custom Item at a specific position.")]
    public void SpawnPositionCommand(
        [CommandParameter("ID", "ID of the Custom Item")] ushort itemId, 
        [CommandParameter("Position", "The position to spawn the item at.")] Vector3 position)
    {
        if (!CustomItemRegistry.TryGetHandler(itemId, out var handler))
        {
            Fail($"Unknown Custom Item: {itemId}");
            return;
        }

        var item = handler.Spawn(position);
        
        Ok($"Spawned item \"{handler.Name}\" at {position.ToPreciseString()} ({Sender.Position.DistanceTo(position)}m), serial: {item.Pickup.Info.Serial})");
    }

    [CommandOverload("spawnroom", "Spawns a Custom Item in a room.")]
    public void SpawnRoomCommand(
        [CommandParameter("ID", "ID of the Custom Item.")] ushort itemId, 
        [CommandParameter("Room", "The room to spawn the item in.")] RoomName room)
    {
        if (!CustomItemRegistry.TryGetHandler(itemId, out var handler))
        {
            Fail($"Unknown Custom Item: {itemId}");
            return;
        }

        if (!RoomUtils.TryFindRoom(room, null, null, out var target))
        {
            Fail($"Unknown room: {room}");
            return;
        }

        var item = handler.Spawn(target.WorldspaceBounds.center, Quaternion.identity);
        
        Ok($"Spawned item \"{handler.Name}\" in room \"{target.Name} ({target.Zone})\" " +
           $"({Vector3.Distance(target.WorldspaceBounds.center, Sender.Position)}m), serial: {item.Pickup.Info.Serial}");
    }

    [CommandOverload("remove", "Removes a Custom Item.")]
    public void RemoveCommand(
        [CommandParameter("Serial", "Serial number of the Custom Item to remove.")] ushort itemSerial)
    {
        if (InventoryExtensions.ServerTryGetItemWithSerial(itemSerial, out var item))
        {
            CustomItemUtils.ForEachInventoryBehaviour(item.ItemSerial, behaviour => behaviour.Destroy(false));
            
            item.DestroyItem();

            Ok($"Destroyed item \"{item.ItemTypeId}\" ({item.ItemSerial}).");
        }
        else if (ExMap.Pickups.TryGetFirst(x => x.Info.Serial == itemSerial, out var pickup))
        {
            CustomItemUtils.ForEachPickupBehaviour(pickup.Info.Serial, behaviour => behaviour.Destroy(false));
            
            pickup.DestroySelf();

            Ok($"Destroyed pickup \"{pickup.Info.ItemId}\" ({pickup.Info.Serial}).");
        }
        else
        {
            Fail($"Could not find item or pickup with serial {itemSerial}.");
        }
    }
    
    [CommandOverload("list", "Lists all registered Custom Items.")]
    public void ListCommand()
    {
        Ok(x =>
        {
            x.AppendLine($"Showing {CustomItemRegistry.Handlers.Count} registered Custom Item(s).");

            foreach (var pair in CustomItemRegistry.Handlers)
                x.AppendLine($"[{pair.Value.Id}]: {pair.Value.Name} ({pair.Value.Description})");
        });
    }

    [CommandOverload("owned", "Shows a list of spawned Custom Items owned by a specific player.")]
    public void OwnedCommand(
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        var items = DictionaryPool<CustomItemHandler, List<CustomItemBehaviour>>.Shared.Rent();
        var player = target ?? Sender;

        foreach (var item in player.Inventory.Items)
        {
            if (item == null)
                continue;
            
            CustomItemUtils.ForEachInventoryBehaviour(item.ItemSerial, behaviour =>
            {
                if (behaviour.Handler is null)
                    return;
                
                if (!items.TryGetValue(behaviour.Handler, out var list))
                    items.Add(behaviour.Handler, list = ListPool<CustomItemBehaviour>.Shared.Rent());
                
                if (!list.Contains(behaviour))
                    list.Add(behaviour);
            });
        }

        foreach (var pickup in ExMap.Pickups)
        {
            if (pickup == null)
                continue;
            
            CustomItemUtils.ForEachPickupBehaviour(pickup.Info.Serial, behaviour =>
            {
                if (behaviour.Handler is null)
                    return;
                
                if (!items.TryGetValue(behaviour.Handler, out var list))
                    items.Add(behaviour.Handler, list = ListPool<CustomItemBehaviour>.Shared.Rent());
                
                if (!list.Contains(behaviour))
                    list.Add(behaviour);
            });
        }
        
        Ok(x =>
        {
            x.AppendLine(
                $"Player \"{player.Nickname}\" ({player.ClearUserId}) owns {items.Count} different Custom Items:");

            foreach (var pair in items)
            {
                x.AppendLine($"{pair.Key.Name} ({pair.Key.Id}):");

                foreach (var behaviour in pair.Value)
                {
                    if (behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                    {
                        x.AppendLine($"> [{inventoryBehaviour.Item.ItemSerial}] {inventoryBehaviour.Item.ItemTypeId} " +
                                     $"({inventoryBehaviour.Item.GetInventorySlot()})");
                    }
                    else if (behaviour is CustomItemPickupBehaviour pickupBehaviour)
                    {
                        x.AppendLine($"> [{pickupBehaviour.Pickup.Info.Serial}] {pickupBehaviour.Pickup.Info.ItemId} " +
                                     $"({(pickupBehaviour.Pickup.Position.TryGetRoom(out var room) ? $"Room: {room.Name} (in {room.Zone})" : "Room: Unknown")} " +
                                     $"[{player.Position.DistanceTo(pickupBehaviour.Pickup.Position)}m]");
                    }
                }
            }
        });

        foreach (var pair in items)
            ListPool<CustomItemBehaviour>.Shared.Return(pair.Value);
        
        DictionaryPool<CustomItemHandler, List<CustomItemBehaviour>>.Shared.Return(items);
    }
}