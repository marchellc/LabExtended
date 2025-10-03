using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.Custom.Items;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using MapGeneration;

using UnityEngine;

namespace LabExtended.Commands.Custom.CustomItems
{
    /// <summary>
    /// Commands for managing custom items.
    /// </summary>
    [Command("customitems", "Manages custom items", "ci")]
    public class CustomItemsCommand : CommandBase, IServerSideCommand
    {
        /// <summary>
        /// Lists all active custom item instances owned by a specific player.
        /// </summary>
        [CommandOverload("inv", "Lists all active custom instances owned by a specific player.")]
        public void Inventory(
            [CommandParameter("Player", "The targeted player (defaults to you).")] ExPlayer? target = null)
        {
            target ??= Sender;

            var itemsDict = DictionaryPool<ItemBase, CustomItem>.Shared.Rent();
            var pickupDict = DictionaryPool<ItemPickupBase, CustomItem>.Shared.Rent();

            foreach (var item in target.Inventory.Items)
            {
                if (item != null && CustomItem.IsCustomItem(item.ItemSerial, out var customItem))
                {
                    itemsDict[item] = customItem;
                }
            }

            foreach (var pickup in target.Inventory.DroppedItems)
            {
                if (pickup != null && CustomItem.IsCustomItem(pickup.Info.Serial, out var customItem))
                {
                    pickupDict[pickup] = customItem;
                }
            }

            if (itemsDict.Count == 0 && pickupDict.Count == 0)
            {
                DictionaryPool<ItemBase, CustomItem>.Shared.Return(itemsDict);
                DictionaryPool<ItemPickupBase, CustomItem>.Shared.Return(pickupDict);

                Fail($"Player {target.ToCommandString()} has no active custom item instances.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                if (itemsDict.Count > 0)
                {
                    x.AppendLine($"- Inventory:");

                    foreach (var pair in itemsDict)
                        x.AppendLine($"  >- [{pair.Key.ItemSerial}] {pair.Key.ItemTypeId} ({pair.Value.Id} / {pair.Value.Name})");

                    if (pickupDict.Count > 0)
                        x.AppendLine();
                }

                if (pickupDict.Count > 0)
                {
                    x.AppendLine($"- Dropped:");

                    foreach (var pair in pickupDict)
                    {
                        RoomIdentifier? room = null;
                        RoomUtils.TryGetRoom(pair.Key.Position, out room);

                        x.AppendLine(
                            $"  >- [{pair.Key.Info.Serial}] {pair.Key.Info.ItemId} ({pair.Value.Id} / {pair.Value.Name})\n" +
                            $"    - Distance: {Sender.Position.DistanceTo(pair.Key.Position)}m\n" +
                            $"    - Room: {room?.Name ?? RoomName.Unnamed} ({room?.Zone ?? FacilityZone.None})");
                    }
                }

                DictionaryPool<ItemBase, CustomItem>.Shared.Return(itemsDict);
                DictionaryPool<ItemPickupBase, CustomItem>.Shared.Return(pickupDict);
            });
        }

        /// <summary>
        /// Lists all registered custom items.
        /// </summary>
        [CommandOverload("list", "Lists all registered custom items.")]
        public void List()
        {
            if (CustomItem.RegisteredItems.Count == 0)
            {
                Fail($"No items have been registered.");
                return;
            }

            Ok(x =>
            {
                foreach (var pair in CustomItem.RegisteredItems)
                {
                    var type = pair.Value.GetType();
                    var baseType = type.BaseType ?? type;

                    x.AppendLine($"- [ID: {pair.Value.Id}] {pair.Value.Name} ({type.Name} / {baseType.Name}, from: {type.Assembly.GetName().Name})");
                }
            });
        }

        /// <summary>
        /// Lists all active custom item instances.
        /// </summary>
        [CommandOverload("active", "Lists all active custom item instances.")]
        public void Active()
        {
            if (CustomItem.RegisteredItems.Count == 0)
            {
                Fail($"No items have been registered.");
                return;
            }

            Ok(x =>
            {
                foreach (var pair in CustomItem.RegisteredItems)
                {
                    var baseType = pair.Value.GetType().BaseType ?? pair.Value.GetType();

                    x.AppendLine($"- [CI: {baseType.Name}] {pair.Value.Name} ({pair.Value.Id})");

                    foreach (var tracker in pair.Value.trackers)
                    {
                        if (tracker.ValidateTracker())
                        {
                            if (tracker.Item != null)
                            {
                                x.AppendLine(
                                    $"  >- [ITEM: {tracker.TargetSerial}] {tracker.Item.ItemTypeId}\n" +
                                    $"    - Player: {tracker.Owner?.ToCommandString() ?? "(null)"}");

                            }

                            if (tracker.Pickup != null)
                            {
                                RoomIdentifier? room = null;
                                RoomUtils.TryGetRoom(tracker.Pickup.Position, out room);

                                x.AppendLine(
                                    $"  >- [PICKUP: {tracker.TargetSerial}] {tracker.Pickup.Info.ItemId}\n" +
                                    $"    - Room: {room?.Name ?? RoomName.Unnamed} ({room?.Zone ?? FacilityZone.None})\n" +
                                    $"    - Distance: {Sender.Position.DistanceTo(tracker.Pickup.Position)}m");
                            }
                        }
                        else
                        {
                            x.AppendLine($"  >- [INVALID: {tracker.TargetSerial}]");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Destroys an active instance of a custom item.
        /// </summary>
        [CommandOverload("destroy", "Destroys an active instance of a custom item.")]
        public void Destroy(
            [CommandParameter("Serial", "The serial number of the item to destroy. Specify 0 to destroy all.")] ushort itemSerial)
        {
            if (CustomItem.RegisteredItems.Count == 0)
            {
                Fail($"No items have been registered.");
                return;
            }

            if (itemSerial == 0)
            {
                Ok(x =>
                {
                    x.AppendLine();

                    foreach (var pair in CustomItem.RegisteredItems)
                        x.AppendLine($"- [ID: {pair.Key}] {pair.Value.Name}; {pair.Value.DestroyInstances()} instance(s) destroyed!");
                });
            }
            else
            {
                if (InventoryExtensions.ServerTryGetItemWithSerial(itemSerial, out var item))
                {
                    if (CustomItem.IsCustomItem(item.ItemSerial, out var customItem))
                    {
                        if (customItem.DestroyItem(item))
                        {
                            Ok($"Destroyed item instance '{itemSerial}' ({item.ItemTypeId}) ({customItem.Id} / {customItem.Name}).");
                        }
                        else
                        {
                            Fail($"Could not destroy item instance '{itemSerial}' ({customItem.Id} / {customItem.Name})");
                        }
                    }
                    else
                    {
                        Fail($"Item with serial number '{itemSerial}' is not a custom item.");
                    }
                }
                else if (ExMap.Pickups.TryGetFirst(x => x != null && x.Info.Serial == itemSerial, out var pickup))
                {
                    if (CustomItem.IsCustomItem(pickup.Info.Serial, out var customItem))
                    {
                        if (customItem.DestroyItem(pickup))
                        {
                            Ok($"Destroyed pickup instance '{itemSerial}' ({pickup.Info.ItemId}) ({customItem.Id} / {customItem.Name}).");
                        }
                        else
                        {
                            Fail($"Could not destroy pickup instance '{itemSerial}' ({customItem.Id} / {customItem.Name})");
                        }
                    }
                    else
                    {
                        Fail($"Pickup with serial number '{itemSerial}' is not a custom item.");
                    }
                }
                else
                {
                    Fail($"Serial '{itemSerial}' does not belong to a valid item or pickup.");
                }
            }
        }

        /// <summary>
        /// Adds a custom item to a player's inventory.
        /// </summary>
        [CommandOverload("add", "Adds a custom item to a player's inventory.")]
        public void Add(
            [CommandParameter("ID", "The ID of the custom item to add.")] string itemId,
            [CommandParameter("Target", "The player to add the item to (defaults to you).")] ExPlayer? target = null)
        {
            target ??= Sender;

            if (CustomItem.RegisteredItems.Count == 0)
            {
                Fail($"No items have been registered.");
                return;
            }

            if (!CustomItem.RegisteredItems.TryGetValue(itemId, out var customItem))
            {
                Fail($"Unknown custom item ID");
                return;
            }

            var addedItem = customItem.AddItem(target);

            if (addedItem != null)
            {
                Ok($"Added custom item '{customItem.Id}' ({customItem.Name}) to inventory of {target.ToCommandString()} (Serial: {addedItem.ItemSerial}, Type: {addedItem.ItemTypeId})");
            }
            else
            {
                Fail($"Could not add custom item '{customItem.Id}' ({customItem.Name}) to inventory of {target.ToCommandString()}");
            }
        }

        /// <summary>
        /// Spawns a custom item at a specific position.
        /// </summary>
        [CommandOverload("spawn", "Spawns a custom item at a specific position.")]
        public void Spawn(
            [CommandParameter("ID", "The ID of the custom item to spawn.")] string itemId,
            [CommandParameter("Position", "The position to spawn the item at.")] Vector3 position)
        {
            if (CustomItem.RegisteredItems.Count == 0)
            {
                Fail($"No items have been registered.");
                return;
            }

            if (!CustomItem.RegisteredItems.TryGetValue(itemId, out var customItem))
            {
                Fail($"Unknown custom item ID");
                return;
            }

            var spawnedItem = customItem.SpawnItem(position, Sender.Rotation);

            if (spawnedItem != null)
            {
                Ok($"Spawned custom item '{customItem.Id}' ({customItem.Name}) at {position.ToPreciseString()} (Serial: {spawnedItem.Info.Serial}, Type: {spawnedItem.Info.ItemId})");
            }
            else
            {
                Fail($"Could not spawn custom item '{customItem.Id}' ({customItem.Name})");
            }
        }
    }
}