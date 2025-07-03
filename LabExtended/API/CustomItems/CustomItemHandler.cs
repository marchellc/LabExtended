using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using LabExtended.API.CustomItems.Behaviours;
using LabExtended.API.CustomItems.Properties;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

using UnityEngine;

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.CustomItems;

/// <summary>
/// Represents an instance of a Custom Item
/// </summary>
public abstract class CustomItemHandler
{
    private InstancePool<CustomItemInventoryBehaviour>? InventoryPool;
    private InstancePool<CustomItemPickupBehaviour>? PickupPool;

    private Func<CustomItemInventoryBehaviour?>? InventoryFactory;
    private Func<CustomItemPickupBehaviour?>? PickupFactory;

    internal Dictionary<ushort, CustomItemInventoryBehaviour>? inventoryItems;
    internal Dictionary<ushort, CustomItemPickupBehaviour>? pickupItems;
    
    /// <summary>
    /// Gets a dictionary of all active inventory items.
    /// </summary>
    public IReadOnlyDictionary<ushort, CustomItemInventoryBehaviour>? InventoryItems => inventoryItems;
    
    /// <summary>
    /// Gets a dictionary of all active pickup items.
    /// </summary>
    public IReadOnlyDictionary<ushort, CustomItemPickupBehaviour>? PickupItems => pickupItems;
    
    /// <summary>
    /// Gets the custom item's ID.
    /// </summary>
    public abstract ushort Id { get; }
    
    /// <summary>
    /// Gets the custom item's name.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Gets the custom item's description.
    /// </summary>
    public abstract string Description { get; }
    
    /// <summary>
    /// Gets the item's inventory properties.
    /// </summary>
    public abstract CustomItemInventoryProperties? InventoryProperties { get; }
    
    /// <summary>
    /// Gets the item's pickup properties.
    /// </summary>
    public abstract CustomItemPickupProperties? PickupProperties { get; }

    /// <summary>
    /// Gets the custom item's pickup behaviour type.
    /// </summary>
    public virtual Type PickupBehaviourType { get; } = typeof(CustomItemPickupBehaviour);

    /// <summary>
    /// Gets the custom item's inventory behaviour type.
    /// </summary>
    public virtual Type InventoryBehaviourType { get; } = typeof(CustomItemInventoryBehaviour);

    /// <summary>
    /// Gets called once the handler is registered.
    /// </summary>
    public virtual void OnRegistered()
    {
        inventoryItems = DictionaryPool<ushort, CustomItemInventoryBehaviour>.Shared.Rent();
        pickupItems = DictionaryPool<ushort, CustomItemPickupBehaviour>.Shared.Rent();

        InventoryPool = new();
        PickupPool = new();

        InventoryFactory = () => Activator.CreateInstance(InventoryBehaviourType) as CustomItemInventoryBehaviour;
        PickupFactory = () => Activator.CreateInstance(PickupBehaviourType) as CustomItemPickupBehaviour;
    }

    /// <summary>
    /// Gets called once the handler is unregistered.
    /// </summary>
    public virtual void OnUnregistered()
    {
        if (inventoryItems != null)
            DictionaryPool<ushort, CustomItemInventoryBehaviour>.Shared.Return(inventoryItems);

        if (pickupItems != null)
            DictionaryPool<ushort, CustomItemPickupBehaviour>.Shared.Return(pickupItems);
        
        InventoryPool?.Dispose();
        InventoryPool = null;
        
        PickupPool?.Dispose();
        PickupPool = null;
        
        InventoryFactory = null;
        PickupFactory = null;

        inventoryItems = null;
        pickupItems = null;
    }

    /// <summary>
    /// Checks if this handler owns a specific custom item.
    /// </summary>
    /// <param name="itemSerial">The target item serial.</param>
    /// <returns>true if the specified serial is owned by this custom item</returns>
    public bool IsItem(ushort itemSerial)
        => CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var behaviour)
           && behaviour.Handler != null
           && behaviour.Handler == this;

    /// <summary>
    /// Checks if this handler owns a specific custom item.
    /// </summary>
    /// <param name="item">The target item.</param>
    /// <returns>true if the specified item is owned by this custom item</returns>
    public bool IsItem(Item item)
        => item?.Base != null && IsItem(item.Serial);

    /// <summary>
    /// Checks if this handler owns a specific custom item.
    /// </summary>
    /// <param name="item">The target item.</param>
    /// <returns>true if the specified item is owned by this custom item</returns>
    public bool IsItem(ItemBase item)
        => item != null && IsItem(item.ItemSerial);

    /// <summary>
    /// Checks if this handler owns a specific custom item pickup.
    /// </summary>
    /// <param name="pickup">The target pickup.</param>
    /// <returns>true if the specified pickup is owned by this custom item</returns>
    public bool IsItem(Pickup pickup)
        => pickup?.Base != null && IsItem(pickup.Serial);
    
    /// <summary>
    /// Checks if this handler owns a specific custom item pickup.
    /// </summary>
    /// <param name="pickup">The target pickup.</param>
    /// <returns>true if the specified pickup is owned by this custom item</returns>
    public bool IsItem(ItemPickupBase pickup)
        => pickup != null && IsItem(pickup.Info.Serial);
    
    /// <summary>
    /// Gets called once per frame (if the handler is registered).
    /// </summary>
    public virtual void OnUpdate() { }
    
    /// <summary>
    /// Gets called once the behaviour of an item is destroyed.
    /// </summary>
    /// <param name="item">The behaviour that is being destroyed.</param>
    public virtual void OnItemDestroyed(CustomItemInventoryBehaviour item) { }
    
    /// <summary>
    /// Gets called once the behaviour of a pickup is destroyed.
    /// </summary>
    /// <param name="pickup">The behaviour that is being destroyed.</param>
    public virtual void OnPickupDestroyed(CustomItemPickupBehaviour pickup) { }
    
    /// <summary>
    /// Gets called when a new pickup behaviour is created.
    /// </summary>
    /// <param name="item">The item that was dropped, can be null.</param>
    /// <param name="pickup">The pickup behaviour to initialize.</param>
    public virtual void InitializePickup(CustomItemPickupBehaviour pickup, CustomItemInventoryBehaviour? item = null) { }
    
    /// <summary>
    /// Gets called when a new item behaviour is created.
    /// </summary>
    /// <param name="pickup">The pickup that was picked up, can be null.</param>
    /// <param name="item">The item behaviour to initialize.</param>
    public virtual void InitializeItem(CustomItemInventoryBehaviour item, CustomItemPickupBehaviour? pickup = null) { }

    /// <summary>
    /// Gives the custom item to a specific player.
    /// </summary>
    /// <param name="targetPlayer">The player to add the item to.</param>
    /// <returns>The added inventory item.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public virtual CustomItemInventoryBehaviour Give(ExPlayer targetPlayer)
    {
        if (targetPlayer is null)
            throw new ArgumentNullException(nameof(targetPlayer));
        
        if (InventoryProperties is null)
            throw new Exception($"Custom Item {Id} ({Name}) does not have any inventory properties defined!");

        if (InventoryProperties.Type is ItemType.None)
            throw new Exception($"Custom Item {Id} ({Name}) set it's inventory type to None!");

        var item = targetPlayer.Inventory.AddItem(InventoryProperties.Type, ItemAddReason.AdminCommand);

        if (item is null)
            throw new Exception($"Could not create an instance of item {InventoryProperties.Type}");

        return ToItem(targetPlayer, item, null);
    }

    /// <summary>
    /// Spawns a new instance of this Custom Item.
    /// </summary>
    /// <returns>The spawned pickup instance.</returns>
    public virtual CustomItemPickupBehaviour Spawn(Vector3 position, Quaternion? rotation = null, ExPlayer? owner = null)
    {
        if (PickupProperties is null)
            throw new Exception($"Custom Item {Id} ({Name}) does not have any pickup properties defined!");

        if (PickupProperties.Type is ItemType.None)
            throw new Exception($"Custom Item {Id} ({Name}) set it's pickup type to None!");
        
        var pickup = ExMap.SpawnItem<ItemPickupBase>(PickupProperties.Type, position, PickupProperties.Scale,
            rotation ?? Quaternion.identity);

        if (pickup == null)
            throw new ArgumentNullException($"Could not spawn pickup of type {PickupProperties.Type}");

        return ToPickup(owner, pickup, null);
    }

    internal CustomItemPickupBehaviour ThrowItem(CustomItemInventoryBehaviour item, float force)
    {
        if (PickupProperties is null)
            throw new Exception($"Custom Item {Id} ({Name}) does not have any pickup properties defined!");

        if (PickupProperties.Type is ItemType.None)
            throw new Exception($"Custom Item {Id} ({Name}) set it's pickup type to None!");

        var pickup = item.Player.Inventory.ThrowItem<ItemPickupBase>(PickupProperties.Type, force,
            PickupProperties.Scale, item.Item.ItemSerial);
        var behaviour = ToPickup(item.Player, pickup, item);
        
        item.OnDropping(new(item.Player.ReferenceHub, item.Item, true));
        item.OnDropped(new(item.Player.ReferenceHub, pickup, true), behaviour);
        
        item.OnRemoved(behaviour);
        
        DestroyItem(item);
        
        item.Item?.DestroyItem();
        return behaviour;
    }

    internal CustomItemPickupBehaviour DropItem(CustomItemInventoryBehaviour item)
    {
        if (PickupProperties is null)
            throw new Exception($"Custom Item {Id} ({Name}) does not have any pickup properties defined!");

        if (PickupProperties.Type is ItemType.None)
            throw new Exception($"Custom Item {Id} ({Name}) set it's pickup type to None!");
        
        var pickup = ExMap.SpawnItem<ItemPickupBase>(PickupProperties.Type, item.Player.Position, PickupProperties.Scale, 
            item.Player.Rotation, item.Item.ItemSerial);
        var behaviour = ToPickup(item.Player, pickup, item);
        
        item.OnDropping(new(item.Player.ReferenceHub, item.Item, false));
        item.OnDropped(new(item.Player.ReferenceHub, pickup, false), behaviour);
        
        item.OnRemoved(behaviour);
        
        DestroyItem(item);
        
        item.Item?.DestroyItem();
        return behaviour;
    }
    
    internal CustomItemPickupBehaviour SpawnItem(CustomItemInventoryBehaviour item, Vector3 position, Quaternion? rotation = null)
    {
        if (PickupProperties is null)
            throw new Exception($"Custom Item {Id} ({Name}) does not have any pickup properties defined!");

        if (PickupProperties.Type is ItemType.None)
            throw new Exception($"Custom Item {Id} ({Name}) set it's pickup type to None!");
        
        var pickup = ExMap.SpawnItem<ItemPickupBase>(PickupProperties.Type, position, PickupProperties.Scale, 
            rotation ?? Quaternion.identity, item.Item.ItemSerial);
        var behaviour = ToPickup(item.Player, pickup, item);
        
        item.OnDropping(new(item.Player.ReferenceHub, item.Item, false));
        item.OnDropped(new(item.Player.ReferenceHub, pickup, false), behaviour);
        
        item.OnRemoved(behaviour);
        
        DestroyItem(item);
        
        item.Item?.DestroyItem();
        return behaviour;
    }

    internal void DestroyItem(CustomItemInventoryBehaviour item)
    {
        if (CustomItemRegistry.Behaviours.TryGetValue(item.Item.ItemSerial, out var behaviour)
            && behaviour == item)
            CustomItemRegistry.Behaviours.Remove(item.Item.ItemSerial);
        
        inventoryItems.Remove(item.Item.ItemSerial);

        item.IsEnabled = false;
        item.OnDisabled();
        
        OnItemDestroyed(item);
        
        InventoryPool.Return(item);
    }

    internal void DestroyPickup(CustomItemPickupBehaviour pickup)
    {
        if (CustomItemRegistry.Behaviours.TryGetValue(pickup.Pickup.Info.Serial, out var behaviour)
            && behaviour == pickup)
            CustomItemRegistry.Behaviours.Remove(pickup.Pickup.Info.Serial);
        
        pickupItems.Remove(pickup.Pickup.Info.Serial);

        pickup.IsEnabled = false;
        pickup.OnDisabled();
        
        OnPickupDestroyed(pickup);
        
        PickupPool.Return(pickup);
    }

    internal virtual CustomItemPickupBehaviour ToPickup(ExPlayer player, ItemPickupBase pickup, CustomItemInventoryBehaviour item)
    {
        var pickupBehaviour = PickupPool.Rent(PickupFactory);

        if (pickupBehaviour is null)
            throw new Exception($"Could not create pickup behaviour {PickupBehaviourType.FullName}");
        
        pickupBehaviour.Pickup = pickup;
        pickupBehaviour.Player = player;
        
        pickupBehaviour.Handler = this;
        
        InternalInitializePickup(pickupBehaviour, item);
        InitializePickup(pickupBehaviour, item);
        
        pickupBehaviour.OnEnabled();
        pickupBehaviour.IsEnabled = true;
        
        pickupBehaviour.OnSpawned(item);
        
        pickupItems[pickup.Info.Serial] = pickupBehaviour;

        CustomItemRegistry.Behaviours[pickup.Info.Serial] = pickupBehaviour;
        return pickupBehaviour;
    }

    internal virtual CustomItemInventoryBehaviour ToItem(ExPlayer player, ItemBase item, CustomItemPickupBehaviour pickup)
    {
        var inventoryBehaviour = InventoryPool.Rent(InventoryFactory);
        
        if (inventoryBehaviour is null)
            throw new Exception($"Could not create inventory behaviour {InventoryBehaviourType.FullName}");

        inventoryBehaviour.Item = item;
        inventoryBehaviour.Player = player;
        
        inventoryBehaviour.Handler = this;
        
        InternalInitializeItem(inventoryBehaviour, pickup);
        InitializeItem(inventoryBehaviour, pickup);
        
        inventoryBehaviour.OnEnabled();
        inventoryBehaviour.IsEnabled = true;
        
        inventoryBehaviour.OnAdded(pickup);
        
        inventoryItems[item.ItemSerial] = inventoryBehaviour;

        CustomItemRegistry.Behaviours[item.ItemSerial] = inventoryBehaviour;
        return inventoryBehaviour;
    }

    internal virtual void InternalInitializeItem(CustomItemInventoryBehaviour item, CustomItemPickupBehaviour? pickup)
    {
        
    }

    internal virtual void InternalInitializePickup(CustomItemPickupBehaviour pickup, CustomItemInventoryBehaviour? item)
    {
        
    }

    internal void Update()
    {
        OnUpdate();
        
        foreach (var inventoryItem in inventoryItems)
        {
            if (!inventoryItem.Value.IsEnabled)
                continue;
            
            inventoryItem.Value.OnUpdate();
        }

        foreach (var pickupItem in pickupItems)
        {
            if (!pickupItem.Value.IsEnabled)
                continue;
            
            pickupItem.Value.OnUpdate();
        }
    }
}