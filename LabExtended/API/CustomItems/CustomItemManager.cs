using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Attributes;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Used to manage custom item instances.
/// </summary>
public static class CustomItemManager
{
    /// <summary>
    /// Called when a new Custom Item instance is created.
    /// </summary>
    public static event Action<CustomItemInstance>? OnItemCreated;
    
    /// <summary>
    /// Called when an existing Custom Item instance is destroyed.
    /// </summary>
    public static event Action<CustomItemInstance>? OnItemDestroyed; 
    
    /// <summary>
    /// Gets all registered custom items.
    /// </summary>
    public static Dictionary<Type, CustomItemData> RegisteredItems { get; } = new();
    
    /// <summary>
    /// Gets all active inventory custom items.
    /// </summary>
    public static Dictionary<ItemBase, CustomItemInstance> InventoryItems { get; } = new();
    
    /// <summary>
    /// Gets all active pickup items.
    /// </summary>
    public static Dictionary<ItemPickupBase, CustomItemInstance> PickupItems { get; } = new();

    /// <summary>
    /// Throws the specified custom item.
    /// </summary>
    /// <param name="item">The item to throw.</param>
    public static void ThrowItem(CustomItemInstance item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Item is null)
            throw new Exception("Custom Item is not in inventory.");

        item.Player.customItems.Remove(item.Item);
        
        item.Pickup = item.Player.Inventory.ThrowItem<ItemPickupBase>(item.Item);
        item.OnThrown();
    }
    
    /// <summary>
    /// Drops the specified custom item.
    /// </summary>
    /// <param name="item">The item to drop.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void DropItem(CustomItemInstance item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Item is null)
            throw new Exception("Custom Item is not in inventory.");

        var isThrow = false;
        
        item.Player.customItems.Remove(item.Item);
        item.Player.ReferenceHub.inventory.ServerRemoveItem(item.ItemSerial, item.Item.PickupDropModel);

        InventoryItems.Remove(item.Item);
        
        item.OnDropping(ref isThrow);
        item.Item = null;
        
        if (item.CustomData.PickupType is ItemType.None)
            return;

        var pickup = ExMap.SpawnItem(item.CustomData.PickupType, item.Player.Position,
            item.CustomData.PickupScale ?? Vector3.one, item.Player.Rotation, item.ItemSerial);

        item.Pickup = pickup;
        item.OnDropped(false);
        
        PickupItems.Add(pickup, item);
    }

    /// <summary>
    /// Transfers a custom item from current owner to another player.
    /// </summary>
    /// <param name="item">The item to transfer.</param>
    /// <param name="newOwner">The player to transfer the item to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void TransferItem(CustomItemInstance item, ExPlayer newOwner)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        
        if (newOwner is null)
            throw new ArgumentNullException(nameof(newOwner));

        if (item.Item is null)
            throw new Exception("Custom Item is not in inventory.");

        item.OnPickingUp(newOwner);
        
        if (item.Item != null)
        {
            item.Player ??= ExPlayer.Get(item.Item.Owner);
            
            if (item.Player != null)
            {
                item.Player.customItems.Remove(item.Item);
                
                item.Player.ReferenceHub.inventory.UserInventory.Items.Remove(item.ItemSerial);
                item.Player.ReferenceHub.inventory.SendItemsNextFrame = true;
            }

            newOwner.customItems.Add(item.Item, item);
            
            newOwner.ReferenceHub.inventory.UserInventory.Items.Add(item.ItemSerial, item.Item);
            newOwner.ReferenceHub.inventory.SendItemsNextFrame = true;

            item.Player = newOwner;
            item.OnPickedUp();
        }
        else if (item.Pickup != null)
        {
            item.Item = newOwner.Inventory.AddItem(item.CustomData.InventoryType, ItemAddReason.PickedUp, item.ItemSerial);
            
            newOwner.customItems.Add(item.Item, item);

            PickupItems.Remove(item.Pickup);
            InventoryItems.Add(item.Item, item);
            
            item.Pickup = null;
            item.OnPickedUp();
        }
    }

    /// <summary>
    /// Gives a custom item to a player.
    /// </summary>
    /// <param name="target">The player to give the item to.</param>
    /// <param name="addReason">The reason for adding this item.</param>
    /// <typeparam name="T">Type of the item to give.</typeparam>
    /// <returns>The given item.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static T GiveItem<T>(ExPlayer target, ItemAddReason addReason = ItemAddReason.AdminCommand) where T : CustomItemInstance
    {
        return (T)GiveItem(typeof(T), target, addReason);
    }
    
    /// <summary>
    /// Gives a custom item to a player.
    /// </summary>
    /// <param name="itemType">The item type to give.</param>
    /// <param name="target">The player to give the item to.</param>
    /// <param name="addReason">The reason for adding this item.</param>
    /// <returns>The given item.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static CustomItemInstance GiveItem(Type itemType, ExPlayer target, ItemAddReason addReason = ItemAddReason.AdminCommand)
    {
        if (itemType is null)
            throw new ArgumentNullException(nameof(itemType));
        
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (!RegisteredItems.TryGetValue(itemType, out var customItemData))
            throw new ArgumentException($"Item type {itemType} is not registered", nameof(itemType));

        if (customItemData.InventoryType is ItemType.None)
            throw new Exception($"Custom Item {itemType.FullName} cannot be given.");

        var originalItem = target.Inventory.AddItem(customItemData.InventoryType, addReason);

        if (originalItem is null)
            throw new Exception($"Custom Item {itemType.FullName} could not be added");
        
        var customItem = Activator.CreateInstance(customItemData.Type) as CustomItemInstance;

        target.customItems.Add(originalItem, customItem);
        
        customItem.Item = originalItem;
        customItem.ItemSerial = originalItem.ItemSerial;
        
        customItem.Player = target;
        customItem.OnEnabled();
        
        InventoryItems.Add(originalItem, customItem);

#pragma warning disable CS8604 // Possible null reference argument.
        OnItemCreated.InvokeSafe(customItem);
#pragma warning restore CS8604 // Possible null reference argument.
        return customItem;
    }
    
    /// <summary>
    /// Spawns a new custom item.
    /// </summary>
    /// <param name="position">The position to spawn the item at.</param>
    /// <param name="rotation">The rotation to spawn the item with.</param>
    /// <param name="owner">The item's optional owner.</param>
    /// <typeparam name="T">Type of the item to spawn.</typeparam>
    /// <returns>The spawned item instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static T SpawnItem<T>(Vector3 position, Quaternion rotation, ExPlayer owner = null) where T : CustomItemInstance
    {
        return (T)SpawnItem(typeof(T), position, rotation, owner);
    }
    
    /// <summary>
    /// Spawns a new custom item.
    /// </summary>
    /// <param name="itemType">The type of the item to spawn.</param>
    /// <param name="position">The position to spawn the item at.</param>
    /// <param name="rotation">The rotation to spawn the item with.</param>
    /// <param name="owner">The item's optional owner.</param>
    /// <returns>The spawned item instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static CustomItemInstance SpawnItem(Type itemType, Vector3 position, Quaternion rotation, ExPlayer owner = null)
    {
        if (itemType is null)
            throw new ArgumentNullException(nameof(itemType));
        
        if (!RegisteredItems.TryGetValue(itemType, out var customItemData))
            throw new ArgumentException($"Item type {itemType} is not registered", nameof(itemType));

        if (customItemData.PickupType is ItemType.None)
            throw new Exception($"Custom Item {itemType.FullName} cannot be spawned.");

        var item = Activator.CreateInstance(customItemData.Type) as CustomItemInstance;
        var pickup = ExMap.SpawnItem(customItemData.PickupType, position,
            customItemData.PickupScale ?? Vector3.one, rotation, null, true);

        item.Player = owner;
        item.Pickup = pickup;
        
        item.ItemSerial = pickup.Info.Serial;
        item.OnEnabled();
        
        PickupItems.Add(pickup, item);
        
#pragma warning disable CS8604 // Possible null reference argument.
        OnItemCreated.InvokeSafe(item);
#pragma warning restore CS8604 // Possible null reference argument.
        return item;
    }

    /// <summary>
    /// Registers a new custom item.
    /// </summary>
    /// <param name="builder">The item to register.</param>
    /// <typeparam name="T">The item's type.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RegisterItem<T>(Action<CustomItemBuilder> builder) where T : CustomItemInstance
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = new CustomItemBuilder()
            .WithType<T>();
        
        builder(instance);
        
        RegisterItem(instance);
    }
    
    /// <summary>
    /// Registers a new custom item.
    /// </summary>
    /// <param name="customItemBuilder">The custom item to register.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RegisterItem(CustomItemBuilder customItemBuilder)
    {
        if (customItemBuilder is null)
            throw new ArgumentNullException(nameof(customItemBuilder));
        
        RegisterItem(customItemBuilder.Data);
    }

    /// <summary>
    /// Registers a new custom item.
    /// </summary>
    /// <param name="customItemData">The custom item to register.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static void RegisterItem(CustomItemData customItemData)
    {
        if (customItemData is null)
            throw new ArgumentNullException(nameof(customItemData));
        
        if (customItemData.Type is null)
            throw new ArgumentException($"Item's class cannot be null.", nameof(customItemData));
        
        if (string.IsNullOrWhiteSpace(customItemData.Name))
            throw new ArgumentException($"Item's name cannot be null, empty or whitespace.", nameof(customItemData));
        
        if (string.IsNullOrWhiteSpace(customItemData.Description))
            throw new ArgumentException($"Item's description cannot be null, empty or whitespace.", nameof(customItemData));
        
        if (RegisteredItems.ContainsKey(customItemData.Type))
            throw new ArgumentException($"Item's type {customItemData.Type.FullName} has already been registered.", nameof(customItemData));
        
        RegisteredItems.Add(customItemData.Type, customItemData);
    }
    
    /// <summary>
    /// Unregisters a custom item.
    /// </summary>
    /// <typeparam name="T">The type to unregister.</typeparam>
    public static void UnregisterItem<T>() where T : CustomItemInstance
        => UnregisterItem(typeof(T));

    /// <summary>
    /// Unregisters a custom item.
    /// </summary>
    /// <param name="itemType">The type to unregister.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void UnregisterItem(Type itemType)
    {
        if (itemType is null)
            throw new ArgumentNullException(nameof(itemType));

        if (!RegisteredItems.ContainsKey(itemType))
            return;

        RegisteredItems.Remove(itemType);
        
        var pickupsToDestroy = ListPool<ItemPickupBase>.Shared.Rent();
        var itemsToDestroy = ListPool<ItemBase>.Shared.Rent();
        
        PickupItems.ForEach(p =>
        {
            if (p.Value.CustomData.Type == itemType)
                pickupsToDestroy.Add(p.Key);
        });
        
        InventoryItems.ForEach(p =>
        {
            if (p.Value.CustomData.Type == itemType)
            {
                p.Value.OnDisabled();
                
                itemsToDestroy.Add(p.Key);
                
#pragma warning disable CS8604 // Possible null reference argument.
                OnItemDestroyed.InvokeSafe(p.Value);
#pragma warning restore CS8604 // Possible null reference argument.
            }
        });
        
        pickupsToDestroy.ForEach(p => p.DestroySelf());
        itemsToDestroy.ForEach(p => p.Owner?.inventory.ServerRemoveItem(p.ItemSerial, p.PickupDropModel));
        
        ListPool<ItemPickupBase>.Shared.Return(pickupsToDestroy);
        ListPool<ItemBase>.Shared.Return(itemsToDestroy);
    }

    private static void OnItemDespawned(ItemPickupBase pickup)
    {
        if (PickupItems.TryGetValue(pickup, out var customItemInstance) && customItemInstance.Item is null)
        {
            customItemInstance.Pickup = null;
            customItemInstance.OnDisabled();
            
#pragma warning disable CS8604 // Possible null reference argument.
            OnItemDestroyed.InvokeSafe(customItemInstance);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        
        PickupItems.Remove(pickup);
    }

    private static void OnPlayerLeft(ExPlayer? player)
    {
        player.customItems.ForEach(p =>
        {
            player.ReferenceHub.inventory.ServerRemoveItem(p.Key.ItemSerial, p.Key.PickupDropModel);
            
            p.Value.OnDisabled();
            
#pragma warning disable CS8604 // Possible null reference argument.
            OnItemDestroyed.InvokeSafe(p.Value);
#pragma warning restore CS8604 // Possible null reference argument.

            InventoryItems.Remove(p.Key);
        });
        
        player.customItems.Clear();
    }

    private static void OnWaiting()
    {
        PickupItems.Clear();
        InventoryItems.Clear();
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ItemPickupBase.OnPickupDestroyed += OnItemDespawned;
        
        InternalEvents.OnPlayerLeft += OnPlayerLeft;
        InternalEvents.OnRoundWaiting += OnWaiting;
    }
}