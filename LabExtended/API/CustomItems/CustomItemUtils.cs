using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API.CustomItems.Behaviours;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.CustomItems;

/// <summary>
/// Utilities targeting Custom Items.
/// </summary>
public static class CustomItemUtils
{
    /// <summary>
    /// Gets a dictionary of custom item weights per item type.
    /// </summary>
    public static Dictionary<ItemType, float> CustomWeight { get; } = new();

    /// <summary>
    /// Gets the behaviour of a specific item serial (or null).
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <typeparam name="TBehaviour">The behaviour type.</typeparam>
    /// <returns>The item behaviour (or null).</returns>
    public static TBehaviour? GetBehaviour<TBehaviour>(ushort itemSerial) where TBehaviour : CustomItemBehaviour
    {
        if (TryGetBehaviour<TBehaviour>(itemSerial, out var behaviour))
            return behaviour;

        return null;
    }
    
    /// <summary>
    /// Attempts to find a behaviour of a specific type targeting an item serial.
    /// </summary>
    /// <param name="itemSerial">The targeted item serial.</param>
    /// <param name="behaviour">The found behaviour.</param>
    /// <typeparam name="TBehaviour">The type of behaviour.</typeparam>
    /// <returns>true if the behaviour was found</returns>
    public static bool TryGetBehaviour<TBehaviour>(ushort itemSerial, out TBehaviour behaviour) where TBehaviour : CustomItemBehaviour
    {
        behaviour = null;
        
        if (!CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var active))
            return false;

        if (active is not TBehaviour target)
            return false;

        behaviour = target;
        return true;
    }
    
    /// <summary>
    /// Attempts to find a behaviour of a specific type targeting an item serial matching a predicate.
    /// </summary>
    /// <param name="itemSerial">The targeted item serial.</param>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="behaviour">The found behaviour.</param>
    /// <typeparam name="TBehaviour">The type of behaviour.</typeparam>
    /// <returns>true if the behaviour was found</returns>
    public static bool TryGetBehaviour<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, out TBehaviour behaviour) where TBehaviour : CustomItemBehaviour
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        behaviour = null;
        
        if (!CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var active))
            return false;

        if (active is not TBehaviour target)
            return false;

        behaviour = target;
        return true;
    }
    
    /// <summary>
    /// Gets all custom item behaviours matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="collection">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetBehavioursNonAlloc<TBehaviour>(Predicate<TBehaviour> predicate, ICollection<TBehaviour> collection)
        where TBehaviour : CustomItemBehaviour
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var behaviour in CustomItemRegistry.Behaviours)
        {
            if (behaviour.Value is not TBehaviour target)
                continue;
            
            if (!predicate(target))
                continue;
            
            collection.Add(target);
        }
    }
    
    /// <summary>
    /// Fills a collection with all handlers that match a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetHandlersNonAlloc(Predicate<CustomItemHandler> predicate, ICollection<CustomItemHandler> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (!predicate(handler.Value))
                continue;
            
            target.Add((handler.Value));
        }
    }
    
    /// <summary>
    /// Gets the sum of custom item's weight.
    /// </summary>
    /// <param name="type">The item's type.</param>
    /// <param name="itemSerial">The item's serial.</param>
    /// <param name="defaultWeight">The item's default weight.</param>
    /// <returns>The custom item weight if any, otherwise the default weight.</returns>
    public static float GetInventoryCustomWeight(ItemType type, ushort itemSerial, float defaultWeight)
    {
        if (CustomWeight.TryGetValue(type, out var customWeight))
            defaultWeight = customWeight;

        if (!CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var behaviour) 
            || behaviour is not CustomItemInventoryBehaviour inventoryBehaviour
            || !inventoryBehaviour.Handler.InventoryProperties.Weight.HasValue)
            return defaultWeight;

        return inventoryBehaviour.Handler.InventoryProperties.Weight.Value;
    }
    
    /// <summary>
    /// Gets the sum of custom item's weight.
    /// </summary>
    /// <param name="type">The item's type.</param>
    /// <param name="itemSerial">The item's serial.</param>
    /// <param name="defaultWeight">The item's default weight.</param>
    /// <returns>The custom item weight if any, otherwise the default weight.</returns>
    public static float GetPickupCustomWeight(ItemType type, ushort itemSerial, float defaultWeight)
    {
        if (CustomWeight.TryGetValue(type, out var customWeight))
            defaultWeight = customWeight;

        if (!CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var behaviour)
            || behaviour is not CustomItemPickupBehaviour pickupBehaviour)
            return defaultWeight;

        return pickupBehaviour.Handler.PickupProperties.Weight ?? defaultWeight;
    }

    /// <summary>
    /// Whether or not a specific item serial is a custom item.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <returns>true if the given serial belongs to a custom item</returns>
    public static bool IsCustomItem(ushort itemSerial)
        => CustomItemRegistry.Behaviours.ContainsKey(itemSerial);

    /// <summary>
    /// Whether or not a specific item serial is a custom item.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <returns>true if the given serial belongs to a custom item</returns>
    public static bool IsCustomItem<TBehaviour>(ushort itemSerial) where TBehaviour : CustomItemBehaviour
        => CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var behaviour) && behaviour is TBehaviour;

    internal static TValue GetCustomValue<TValue, TBehaviour>(this TBehaviour behaviour,
        Func<TBehaviour, TValue> selector, Func<TValue, bool> validator, TValue defaultValue)
    {
        if (behaviour == null)
            return defaultValue;
        
        var customValue = selector(behaviour);
        
        if (!validator(customValue))
            return defaultValue;
        
        return customValue;
    }

    internal static bool ProcessEvent<TBehaviour>(ushort itemSerial, Action<TBehaviour> action)
    {
        if (CustomItemRegistry.Behaviours.TryGetValue(itemSerial, out var behaviour)
            && behaviour is TBehaviour result)
        {
            action.InvokeSafe(result);
            return true;
        }

        return false;
    }

    internal static CustomItemPickupBehaviour ProcessDropped(this CustomItemInventoryBehaviour inventoryBehaviour, ItemPickupBase pickup, ExPlayer player,
        PlayerDroppedItemEventArgs args)
    {
        if (inventoryBehaviour != null)
        {
            var pickupBehaviour = inventoryBehaviour.Handler.ToPickup(player, pickup, inventoryBehaviour);

            inventoryBehaviour.OnDropped(args, pickupBehaviour);
            inventoryBehaviour.OnRemoved(pickupBehaviour);

            inventoryBehaviour.Handler.DestroyItem(inventoryBehaviour, false);
            return pickupBehaviour;
        }

        return null;
    }

    internal static void ProcessPickedUp(this CustomItemPickupBehaviour pickupBehaviour, ItemBase item, ExPlayer player,
        PlayerPickedUpItemEventArgs args)
    {
        if (pickupBehaviour != null)
        {
            var inventoryBehaviour = pickupBehaviour.Handler.ToItem(player, item, pickupBehaviour);

            pickupBehaviour.OnPickedUp(args, inventoryBehaviour);
            pickupBehaviour.Handler.DestroyPickup(pickupBehaviour, false);
        }
    }
}