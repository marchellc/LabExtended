using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.PlayerEvents;
using LabExtended.API.CustomItems.Behaviours;

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
    /// Gets all pickup behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetPickupBehavioursNonAlloc<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, ICollection<TBehaviour> target)
        where TBehaviour : CustomItemPickupBehaviour
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && pickupBehaviour is TBehaviour castBehaviour
                && predicate(castBehaviour))
            {
                target.Add(castBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all pickup behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetPickupBehavioursNonAlloc<TBehaviour>(ushort itemSerial, ICollection<TBehaviour> target)
        where TBehaviour : CustomItemPickupBehaviour
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && pickupBehaviour is TBehaviour castBehaviour)
            {
                target.Add(castBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all pickup behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetPickupBehavioursNonAlloc(ushort itemSerial, ICollection<CustomItemPickupBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                target.Add(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all pickup behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetPickupBehavioursNonAlloc(ushort itemSerial, Predicate<CustomItemPickupBehaviour> predicate, 
        ICollection<CustomItemPickupBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && predicate(pickupBehaviour))
            {
                target.Add(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all inventory behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetInventoryBehavioursNonAlloc<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, ICollection<TBehaviour> target)
        where TBehaviour : CustomItemInventoryBehaviour
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && inventoryBehaviour is TBehaviour castBehaviour
                && predicate(castBehaviour))
            {
                target.Add(castBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all inventory behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetInventoryBehavioursNonAlloc<TBehaviour>(ushort itemSerial, ICollection<TBehaviour> target)
        where TBehaviour : CustomItemInventoryBehaviour
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && inventoryBehaviour is TBehaviour castBehaviour)
            {
                target.Add(castBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all inventory behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetInventoryBehavioursNonAlloc(ushort itemSerial, ICollection<CustomItemInventoryBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                target.Add(inventoryBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all inventory behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetInventoryBehavioursNonAlloc(ushort itemSerial, Predicate<CustomItemInventoryBehaviour> predicate, 
        ICollection<CustomItemInventoryBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && predicate(inventoryBehaviour))
            {
                target.Add(inventoryBehaviour);
            }
        }
    }
    
        /// <summary>
    /// Gets all behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetBehavioursNonAlloc<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, ICollection<TBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && inventoryBehaviour is TBehaviour castInventoryBehaviour
                && predicate(castInventoryBehaviour))
            {
                target.Add(castInventoryBehaviour);
            }

            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && pickupBehaviour is TBehaviour castPickupBehaviour
                && predicate(castPickupBehaviour))
            {
                target.Add(castPickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetBehavioursNonAlloc<TBehaviour>(ushort itemSerial, ICollection<TBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && inventoryBehaviour is TBehaviour castInventoryBehaviour)
            {
                target.Add(castInventoryBehaviour);
            }

            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && pickupBehaviour is TBehaviour castPickupBehaviour)
            {
                target.Add(castPickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetBehavioursNonAlloc(ushort itemSerial, Predicate<CustomItemBehaviour> predicate, 
        ICollection<CustomItemBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour)
                && predicate(inventoryBehaviour))
            {
                target.Add(inventoryBehaviour);
            }

            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour)
                && predicate(pickupBehaviour))
            {
                target.Add(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Gets all behaviours that target a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetBehavioursNonAlloc(ushort itemSerial, ICollection<CustomItemBehaviour> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                target.Add(inventoryBehaviour);
            }

            if (handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                target.Add(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Fills a collection with all handlers that have behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="target">The target collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetHandlersNonAlloc(ushort itemSerial, ICollection<CustomItemHandler> target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.ContainsKey(itemSerial)
                || handler.Value.pickupItems.ContainsKey(itemSerial))
                target.Add(handler.Value);
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour(ushort itemSerial, Action<CustomItemInventoryBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                action(inventoryBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour<TBehaviour>(ushort itemSerial, Action<TBehaviour> action) where TBehaviour : CustomItemInventoryBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                if (inventoryBehaviour is not TBehaviour behaviour)
                    continue;
                
                action(behaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour of each item in a list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour<TBehaviour>(this IEnumerable<ItemBase> items, Predicate<TBehaviour> predicate, Action<TBehaviour> action) where TBehaviour : CustomItemInventoryBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.inventoryItems.TryGetValue(item.ItemSerial, out var inventoryBehaviour))
                {
                    if (inventoryBehaviour is not TBehaviour behaviour)
                        continue;
                    
                    if (!predicate(behaviour))
                        continue;

                    action(behaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour of each item in a list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour<TBehaviour>(this IEnumerable<ItemBase> items, Action<TBehaviour> action) where TBehaviour : CustomItemInventoryBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.inventoryItems.TryGetValue(item.ItemSerial, out var inventoryBehaviour))
                {
                    if (inventoryBehaviour is not TBehaviour behaviour)
                        continue;

                    action(behaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour of each item in a list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="predicate">The search predicate.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour(this IEnumerable<ItemBase> items, Predicate<CustomItemInventoryBehaviour> predicate, Action<CustomItemInventoryBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.inventoryItems.TryGetValue(item.ItemSerial, out var inventoryBehaviour)
                    && predicate(inventoryBehaviour))
                {
                    action(inventoryBehaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour of each item in a list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour(this IEnumerable<ItemBase> items, Action<CustomItemInventoryBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.inventoryItems.TryGetValue(item.ItemSerial, out var inventoryBehaviour))
                {
                    action(inventoryBehaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour(ushort itemSerial, Predicate<CustomItemInventoryBehaviour> predicate, 
        Action<CustomItemInventoryBehaviour> action)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                if (!predicate(inventoryBehaviour))
                    continue;
                
                action(inventoryBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachInventoryBehaviour<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, 
        Action<TBehaviour> action) where TBehaviour : CustomItemInventoryBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                if (inventoryBehaviour is not TBehaviour behaviour)
                    continue;
                
                if (!predicate(behaviour))
                    continue;
                
                action(behaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour(ushort itemSerial, Action<CustomItemPickupBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                action(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific in an item list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour(this IEnumerable<ItemPickupBase> items, Action<CustomItemPickupBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.pickupItems.TryGetValue(item.Info.Serial, out var pickupBehaviour))
                {
                    action(pickupBehaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific in an item list.
    /// </summary>
    /// <param name="items">The item list.</param>
    /// <param name="predicate">The search predicate.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour(this IEnumerable<ItemPickupBase> items, Predicate<CustomItemPickupBehaviour> predicate, Action<CustomItemPickupBehaviour> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in items)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.pickupItems.TryGetValue(item.Info.Serial, out var pickupBehaviour)
                    && predicate(pickupBehaviour))
                {
                    action(pickupBehaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour in a list of pickups.
    /// </summary>
    /// <param name="pickups">The pickup list.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour<TBehaviour>(this IEnumerable<ItemPickupBase> pickups, Action<TBehaviour> action) where TBehaviour : CustomItemPickupBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var pickup in pickups)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.pickupItems.TryGetValue(pickup.Info.Serial, out var pickupBehaviour))
                {
                    if (pickupBehaviour is not TBehaviour behaviour)
                        continue;

                    action(behaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour in a list of pickups.
    /// </summary>
    /// <param name="pickups">The pickup list.</param>
    /// <param name="predicate">The search predicate.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour<TBehaviour>(this IEnumerable<ItemPickupBase> pickups, Predicate<TBehaviour> predicate, Action<TBehaviour> action) where TBehaviour : CustomItemPickupBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        foreach (var pickup in pickups)
        {
            foreach (var pair in CustomItemRegistry.Handlers)
            {
                if (pair.Value.pickupItems.TryGetValue(pickup.Info.Serial, out var pickupBehaviour))
                {
                    if (pickupBehaviour is not TBehaviour behaviour)
                        continue;
                    
                    if (!predicate(behaviour))
                        continue;

                    action(behaviour);
                }
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour<TBehaviour>(ushort itemSerial, Action<TBehaviour> action) where TBehaviour : CustomItemPickupBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                if (pickupBehaviour is not TBehaviour behaviour)
                    continue;
                
                action(behaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour(ushort itemSerial, Predicate<CustomItemPickupBehaviour> predicate, 
        Action<CustomItemPickupBehaviour> action)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                if (!predicate(pickupBehaviour))
                    continue;
                
                action(pickupBehaviour);
            }
        }
    }
    
    /// <summary>
    /// Executes a delegate for each pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial.</param>
    /// <param name="predicate">The predicate used to filter behaviours.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEachPickupBehaviour<TBehaviour>(ushort itemSerial, Predicate<TBehaviour> predicate, 
        Action<TBehaviour> action) where TBehaviour : CustomItemPickupBehaviour
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                if (pickupBehaviour is not TBehaviour behaviour)
                    continue;
                
                if (!predicate(behaviour))
                    continue;
                
                action(behaviour);
            }
        }
    }
    
    /// <summary>
    /// Attempts to get an inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <typeparam name="TBehaviour">The type to cast the behaviour instance to.</typeparam>
    /// <returns>true if the behaviour instance was found</returns>
    public static bool TryGetInventoryBehaviour<TBehaviour>(ushort itemSerial, out TBehaviour? behaviour)
        where TBehaviour : CustomItemInventoryBehaviour
    {
        behaviour = null;
        
        if (!TryGetInventoryBehaviour(itemSerial, out var behaviourObject)
            || behaviourObject is not TBehaviour castBehaviour)
            return false;
        
        behaviour = castBehaviour;
        return true;
    }
    
    /// <summary>
    /// Attempts to get a pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <typeparam name="TBehaviour">The type to cast the behaviour instance to.</typeparam>
    /// <returns>true if the behaviour instance was found</returns>
    public static bool TryGetPickupBehaviour<TBehaviour>(ushort itemSerial, out TBehaviour? behaviour)
        where TBehaviour : CustomItemPickupBehaviour
    {
        behaviour = null;
        
        if (!TryGetPickupBehaviour(itemSerial, out var behaviourObject)
            || behaviourObject is not TBehaviour castBehaviour)
            return false;
        
        behaviour = castBehaviour;
        return true;
    }
    
    /// <summary>
    /// Attempts to get an inventory behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <returns>true if the behaviour instance was resolved</returns>
    public static bool TryGetInventoryBehaviour(ushort itemSerial, out CustomItemInventoryBehaviour? behaviour)
    {
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                behaviour = inventoryBehaviour;
                return true;
            }
        }

        behaviour = null;
        return false;
    }
    
    /// <summary>
    /// Attempts to get a pickup behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <returns>true if the behaviour instance was resolved</returns>
    public static bool TryGetPickupBehaviour(ushort itemSerial, out CustomItemPickupBehaviour? behaviour)
    {
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                behaviour = pickupBehaviour;
                return true;
            }
        }

        behaviour = null;
        return false;
    }
    
    /// <summary>
    /// Attempts to get a behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <typeparam name="TBehaviour">The type to cast the behaviour instance to.</typeparam>
    /// <returns>true if the behaviour instance was found</returns>
    public static bool TryGetBehaviour<TBehaviour>(ushort itemSerial, out TBehaviour? behaviour)
        where TBehaviour : CustomItemBehaviour
    {
        behaviour = null;
        
        if (!TryGetBehaviour(itemSerial, out var behaviourObject)
            || behaviourObject is not TBehaviour castBehaviour)
            return false;
        
        behaviour = castBehaviour;
        return true;
    }
    
    /// <summary>
    /// Attempts to get a behaviour for a specific item serial.
    /// </summary>
    /// <param name="itemSerial">The item serial number.</param>
    /// <param name="behaviour">The resolved behaviour instance.</param>
    /// <returns>true if the behaviour was found</returns>
    public static bool TryGetBehaviour(ushort itemSerial, out CustomItemBehaviour? behaviour)
    {
        foreach (var pair in CustomItemRegistry.Handlers)
        {
            if (pair.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour))
            {
                behaviour = inventoryBehaviour;
                return true;
            }

            if (pair.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour))
            {
                behaviour = pickupBehaviour;
                return true;
            }
        }

        behaviour = null;
        return false;
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

        var countWeight = 0f;
        var anyCustom = false;
        
        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.InventoryProperties.Weight.HasValue
                && handler.Value.inventoryItems.ContainsKey(itemSerial))
            {
                countWeight += handler.Value.InventoryProperties.Weight.Value;
                anyCustom = true;
            }
        }
        
        return anyCustom ? countWeight : defaultWeight;
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
        
        var countWeight = 0f;
        var anyCustom = false;
        
        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.PickupProperties.Weight.HasValue
                && handler.Value.pickupItems.ContainsKey(itemSerial))
            {
                countWeight += handler.Value.PickupProperties.Weight.Value;
                anyCustom = true;
            }
        }
        
        return anyCustom ? countWeight : defaultWeight;
    }

    /// <summary>
    /// Whether or not a specific item serial is a custom item.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <returns>true if the given serial belongs to a custom item</returns>
    public static bool IsCustomItem(ushort itemSerial)
    {
        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.ContainsKey(itemSerial)
                || handler.Value.pickupItems.ContainsKey(itemSerial))
                return true;
        }

        return false;
    }
    
    /// <summary>
    /// Whether or not a specific item serial is a custom item.
    /// </summary>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <returns>true if the given serial belongs to a custom item</returns>
    public static bool IsCustomItem<TBehaviour>(ushort itemSerial) where TBehaviour : CustomItemBehaviour
    {
        foreach (var handler in CustomItemRegistry.Handlers)
        {
            if (handler.Value.inventoryItems.TryGetValue(itemSerial, out var inventoryBehaviour) && inventoryBehaviour is TBehaviour
                || handler.Value.pickupItems.TryGetValue(itemSerial, out var pickupBehaviour) && pickupBehaviour is TBehaviour)
                return true;
        }

        return false;
    }

    internal static CustomItemInventoryBehaviour SelectItemBehaviour(List<CustomItemInventoryBehaviour> itemBehaviours)
        => itemBehaviours.FirstOrDefault(x =>
            x.Handler != null && x.Handler.PickupProperties != null &&
            x.Handler.PickupProperties.Type != ItemType.None);
    
    internal static CustomItemPickupBehaviour SelectPickupBehaviour(List<CustomItemPickupBehaviour> behaviours)
        => behaviours.FirstOrDefault(x =>
            x.Handler != null && x.Handler.InventoryProperties != null &&
            x.Handler.InventoryProperties.Type != ItemType.None);

    internal static void ProcessDropped(List<CustomItemInventoryBehaviour> inventoryBehaviours, ItemPickupBase pickup, ExPlayer player,
        PlayerDroppedItemEventArgs args, List<CustomItemPickupBehaviour>? newPickups = null)
    {
        for (var i = 0; i < inventoryBehaviours.Count; i++)
        {
            var behaviour = inventoryBehaviours[i];
            var behaviourPickup = behaviour.Handler.ToPickup(player, pickup, behaviour);

            behaviour.OnDropped(args, behaviourPickup);
            behaviour.OnRemoved(behaviourPickup);
            
            behaviour.Handler.DestroyItem(behaviour);
            
            newPickups?.Add(behaviourPickup);
        }
    }

    internal static void ProcessPickedUp(List<CustomItemPickupBehaviour> pickupBehaviours, ItemBase item, ExPlayer player,
        PlayerPickedUpItemEventArgs args)
    {
        for (var i = 0; i < pickupBehaviours.Count; i++)
        {
            var behaviour = pickupBehaviours[i];
            var behaviourItem = behaviour.Handler.ToItem(player, item, behaviour);

            behaviour.OnPickedUp(args, behaviourItem);
            behaviour.Handler.DestroyPickup(behaviour);
        }
    }
}