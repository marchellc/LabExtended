using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.Extensions;
using LabExtended.Attributes;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

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
    /// Throws the specified custom item.
    /// </summary>
    /// <param name="item">The item to throw.</param>
    public static void ThrowItem(CustomItemInstance item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Item is null)
            throw new Exception("Custom Item is not in inventory.");
        
        item.Player.Inventory.ThrowItem<ItemPickupBase>(item.Item);
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

        item.Player.Inventory.DropItem(item.Item);
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
        
        item.Item.TransferItem(newOwner.ReferenceHub);
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

        var tracker = originalItem.GetTracker();
        var customItem = Activator.CreateInstance(customItemData.Type) as CustomItemInstance;

        tracker.CustomItem = customItem;

        customItem.CustomData = customItemData;

        target.customItems.Add(originalItem, customItem);

        customItem.Tracker = tracker;
        customItem.Player = target;
        
        customItem.OnEnabled();

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
    public static T SpawnItem<T>(Vector3 position, Quaternion rotation, ExPlayer? owner = null) where T : CustomItemInstance
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
    public static CustomItemInstance SpawnItem(Type itemType, Vector3 position, Quaternion rotation, ExPlayer? owner = null)
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
        var tracker = pickup.GetTracker();
        
        item.Player = owner;
        item.Tracker = tracker;
        item.CustomData = customItemData;

        tracker.CustomItem = item;

        item.OnEnabled();
        
        
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

        if (!RegisteredItems.Remove(itemType))
            return;

        var trackersToDestroy = ListPool<ItemTracker>.Shared.Rent();

        foreach (var tracker in ItemTracker.Trackers)
        {
            if (tracker.Value.CustomItem != null && tracker.Value.CustomItem.GetType() == itemType)
            {
                trackersToDestroy.Add(tracker.Value);
            }
        }
        
        trackersToDestroy.ForEach(tracker => tracker.Destroy());
        
        ListPool<ItemTracker>.Shared.Return(trackersToDestroy);
    }

    private static void OnTrackerDestroyed(ItemTracker tracker)
    {
        if (tracker.CustomItem != null)
        {
            tracker.CustomItem.OnDisabled();
            
            OnItemDestroyed?.InvokeSafe(tracker.CustomItem);
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ItemTracker.Destroyed += OnTrackerDestroyed;
    }
}