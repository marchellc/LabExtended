using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;

using LabApi.Events.Handlers;

using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Events.Map;
using LabExtended.Extensions;

using LabExtended.Utilities.Testing.CustomItems;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Handles registration of Custom Items.
/// </summary>
public static class CustomItemRegistry
{
    /// <summary>
    /// Gets the list of registered Custom Items.
    /// </summary>
    public static Dictionary<Type, CustomItemHandler> Handlers { get; } = new();

    /// <summary>
    /// Gets the list of active Custom Item behaviours.
    /// </summary>
    public static Dictionary<ushort, CustomItemBehaviour> Behaviours { get; } = new();

    /// <summary>
    /// Attempts to get a handler by it's ID.
    /// </summary>
    /// <param name="id">The handler's ID.</param>
    /// <param name="handler">The resolved handler instance.</param>
    /// <typeparam name="THandler">The handler type to cast to.</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGetHandler<THandler>(ushort id, out THandler? handler) where THandler : CustomItemHandler
        => TryGetHandler(x => x.Id == id, out handler);
    
    /// <summary>
    /// Attempts to get a handler by it's ID.
    /// </summary>
    /// <param name="id">The handler's ID.</param>
    /// <param name="handler">The resolved handler instance.</param>
    /// <returns>true if the handler was found</returns>
    public static bool TryGetHandler(ushort id, out CustomItemHandler? handler)
        => TryGetHandler(x => x.Id == id, out handler);

    /// <summary>
    /// Attempts to get a handler of a specific type.
    /// </summary>
    /// <param name="handler">The resolved handler instance.</param>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <returns>true if the handler was resolved</returns>
    public static bool TryGetHandler<THandler>(out THandler? handler) where THandler : CustomItemHandler
        => (handler = Handlers.GetValueOrDefault(typeof(THandler)) as THandler) != null;
    
    /// <summary>
    /// Attempts to get a handler of a specific type.
    /// </summary>
    /// <param name="type">The type to get.</param>
    /// <param name="handler">The resolved instance.</param>
    /// <returns>true if the instance was found</returns>
    public static bool TryGetHandler(Type type, out CustomItemHandler handler)
        => Handlers.TryGetValue(type, out handler);
    
    /// <summary>
    /// Attempts to get a specific handler.
    /// </summary>
    /// <param name="predicate">The predicate used to search for the handler.</param>
    /// <param name="handler">The handler instance that was resolved.</param>
    /// <typeparam name="THandler">The type of handler to cast to.</typeparam>
    /// <returns>true if the handler was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetHandler<THandler>(Predicate<THandler> predicate, out THandler? handler) where THandler : CustomItemHandler
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        handler = null;

        foreach (var pair in Handlers)
        {
            if (pair.Value is not THandler value)
                continue;
            
            if (!predicate(value))
                continue;
            
            handler = value;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Attempts to get a specific handler.
    /// </summary>
    /// <param name="predicate">The predicate used to search for the handler.</param>
    /// <param name="handler">The handler instance that was resolved.</param>
    /// <returns>true if the handler was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetHandler(Predicate<CustomItemHandler> predicate, out CustomItemHandler? handler)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        handler = null;

        foreach (var pair in Handlers)
        {
            if (!predicate(pair.Value))
                continue;
            
            handler = pair.Value;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Unregisters a Custom Item handler.
    /// </summary>
    /// <typeparam name="THandler">The type of handler to unregister.</typeparam>
    /// <returns>true if the handler was unregistered.</returns>
    public static bool Unregister<THandler>() where THandler : CustomItemHandler
        => Unregister(typeof(THandler));
    
    /// <summary>
    /// Unregisters a Custom Item handler.
    /// </summary>
    /// <param name="type">The type to unregister.</param>
    /// <returns>true if the handler was unregistered</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool Unregister(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!Handlers.TryGetValue(type, out var existingHandler))
            return false;
        
        Handlers.Remove(type);
        
        existingHandler.OnUnregistered();
        return true;
    }
    
    /// <summary>
    /// Registers a new Custom Item.
    /// </summary>
    /// <typeparam name="THandler">The handler's type.</typeparam>
    /// <returns>The created Custom Item handler.</returns>
    /// <exception cref="Exception"></exception>
    public static THandler Register<THandler>() where THandler : CustomItemHandler, new()
    {
        if (Register(typeof(THandler)) is not THandler handler)
            throw new Exception($"Could not register {typeof(THandler).FullName}");
        
        return handler;
    }
    
    /// <summary>
    /// Registers a new Custom Item.
    /// </summary>
    /// <param name="type">The handler's type.</param>
    /// <returns>The created Custom Item handler.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static CustomItemHandler? Register(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (Handlers.TryGetValue(type, out var existingHandler))
            return existingHandler;
        
        if (!typeof(CustomItemHandler).IsAssignableFrom(type))
            throw new Exception($"Type {type.FullName} is not a valid CustomItemHandler");

        if (Activator.CreateInstance(type) is not CustomItemHandler handler)
            throw new Exception($"Could not instantiate type {type.FullName}");
        
        Handlers.Add(type, handler);
        
        handler.OnRegistered();
        return handler;
    }
    
    private static void OnUpdate()
    {
        foreach (var behaviour in Behaviours)
        {
            try
            {
                behaviour.Value.OnUpdate();
            }
            catch (Exception ex)
            {
                ApiLog.Error("Custom Item Registry", $"Could not update behaviour for serial &3{behaviour.Key}&r " +
                                                     $"(&6{behaviour.Value?.Handler?.Id ?? -1}&r - &6{behaviour.Value?.Handler?.Name ?? "(null)"}&r):\n{ex}");
            }
        }
    }

    private static void OnRestart()
    {
        var behaviours = ListPool<CustomItemBehaviour>.Shared.Rent();
        
        foreach (var pair in Behaviours)
            behaviours.Add(pair.Value);

        for (var index = 0; index < behaviours.Count; index++)
        {
            var behaviour = behaviours[index];
            
            if (behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                inventoryBehaviour.Handler.DestroyItem(inventoryBehaviour, false);
            else if (behaviour is CustomItemPickupBehaviour pickupBehaviour)
                pickupBehaviour.Handler.DestroyPickup(pickupBehaviour, false);
        }

        ListPool<CustomItemBehaviour>.Shared.Return(behaviours);
        
        Behaviours.Clear();
    }
    
    private static void OnLeaving(ExPlayer player)
    {
        var inventoryBehaviours = ListPool<CustomItemInventoryBehaviour>.Shared.Rent();
        var pickupBehaviours = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
        
        foreach (var behaviour in Behaviours)
        {
            if (behaviour.Value is CustomItemInventoryBehaviour inventoryBehaviour)
            {
                if (inventoryBehaviour.Player != null && inventoryBehaviour.Player == player)
                {
                    inventoryBehaviours.Add(inventoryBehaviour);
                }
            }
            else if (behaviour.Value is CustomItemPickupBehaviour pickupBehaviour)
            {
                if (pickupBehaviour.Player != null && pickupBehaviour.Player == player)
                {
                    pickupBehaviours.Add(pickupBehaviour);
                }
            }
        }

        for (var i = 0; i < inventoryBehaviours.Count; i++)
        {
            var inventoryBehaviour = inventoryBehaviours[i];

            if (inventoryBehaviour.Handler.InventoryProperties.DropOnOwnerLeave)
                inventoryBehaviour.Drop();
            else
                inventoryBehaviour.Destroy(true);
        }

        for (var i = 0; i < pickupBehaviours.Count; i++)
        {
            var pickupBehaviour = pickupBehaviours[i];
            
            if (pickupBehaviour.Handler.PickupProperties.DestroyOnOwnerLeave)
                pickupBehaviour.Destroy(true);
        }
        
        ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
        ListPool<CustomItemInventoryBehaviour>.Shared.Return(inventoryBehaviours);
    }

    private static void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemInventoryBehaviour>(args.LightItem.Serial, item => item.OnTogglingLight(args));

    private static void OnToggledFlashlight(PlayerToggledFlashlightEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemInventoryBehaviour>(args.LightItem.Serial, item => item.OnToggledLight(args));

    private static void OnFlippingCoin(PlayerFlippingCoinEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemInventoryBehaviour>(args.CoinItem.Serial, item => item.OnFlippingCoin(args));

    private static void OnFlippedCoin(PlayerFlippedCoinEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemInventoryBehaviour>(args.CoinItem.Serial, item => item.OnFlippedCoin(args));

    private static void OnCollided(PickupCollidedEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemPickupBehaviour>(args.Pickup.Info.Serial, pickup => pickup.OnCollided(args));

    private static void OnDisarming(PlayerCuffingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnDisarming(args);
                }    
            }
        }
    }

    private static void OnDisarmed(PlayerCuffedEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnDisarmed(args);
                }    
            }
        }
    }

    private static void OnEscaping(PlayerEscapingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnEscaping(args);
                }    
            }
        }
    }

    private static void OnEscaped(PlayerEscapedEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnEscaped(args);
                }    
            }
        }
    }

    private static void OnHurting(PlayerHurtingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnHurting(args);
                }    
            }
        }
    }

    private static void OnHurt(PlayerHurtEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnHurt(args);
                }    
            }
        }
    }

    private static void OnDying(PlayerDyingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnDying(args);
                }    
            }
        }
    }

    private static void OnDied(PlayerDeathEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnDied(args);
                }    
            }
        }
    }

    private static void OnChangingRole(PlayerChangingRoleEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnChangingRole(args);
                }    
            }
        }
    }

    private static void OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            foreach (var item in player.Inventory.Items)
            {
                if (Behaviours.TryGetValue(item.ItemSerial, out var behaviour)
                    && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
                {
                    inventoryBehaviour.OnChangedRole(args);
                }    
            }
        }
    }
    
    private static void OnUpgradingItem(Scp914ProcessingInventoryItemEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemInventoryBehaviour>(args.Item.Serial, item => item.OnUpgrading(args));

    private static void OnUpgradingPickup(Scp914ProcessingPickupEventArgs args)
        => CustomItemUtils.ProcessEvent<CustomItemPickupBehaviour>(args.Pickup.Serial, item => item.OnUpgrading(args));

    private static void OnUpgradedItem(Scp914ProcessedInventoryItemEventArgs args)
    {
        if (args.Item?.Base != null
            && Behaviours.TryGetValue(args.Item.Serial, out var behaviour)
            && behaviour is CustomItemInventoryBehaviour inventoryBehaviour)
        {
            inventoryBehaviour.Item = args.Item.Base;
            inventoryBehaviour.IsSelected = args.Item.IsEquipped;

            if (args.Item.CurrentOwner is ExPlayer owner)
                inventoryBehaviour.Player = owner;

            inventoryBehaviour.OnUpgraded(args);
        }
    }

    private static void OnUpgradedPickup(Scp914ProcessedPickupEventArgs args)
    {
        if (args.Pickup?.Base != null
            && CustomItemUtils.TryGetBehaviour<CustomItemPickupBehaviour>(args.Pickup.Serial, out var behaviour))
        {
            behaviour.Pickup = args.Pickup.Base;

            if (args.Pickup.LastOwner is ExPlayer player)
                behaviour.Player = player;

            behaviour.OnUpgraded(args);
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        PlayerUpdateHelper.OnUpdate += OnUpdate;
        
        InternalEvents.OnPlayerLeft += OnLeaving;
        InternalEvents.OnRoundRestart += OnRestart;
        
        PlayerEvents.TogglingFlashlight += OnTogglingFlashlight;
        PlayerEvents.ToggledFlashlight += OnToggledFlashlight;
        
        PlayerEvents.FlippingCoin += OnFlippingCoin;
        PlayerEvents.FlippedCoin += OnFlippedCoin;

        PlayerEvents.Cuffing += OnDisarming;
        PlayerEvents.Cuffed += OnDisarmed;
        
        PlayerEvents.Escaping += OnEscaping;
        PlayerEvents.Escaped += OnEscaped;

        PlayerEvents.Hurting += OnHurting;
        PlayerEvents.Hurt += OnHurt;

        PlayerEvents.Dying += OnDying;
        PlayerEvents.Death += OnDied;

        PlayerEvents.ChangingRole += OnChangingRole;
        PlayerEvents.ChangedRole += OnChangedRole;
        
        ExMapEvents.PickupCollided += OnCollided;

        Scp914Events.ProcessingInventoryItem += OnUpgradingItem;
        Scp914Events.ProcessedInventoryItem += OnUpgradedItem;

        Scp914Events.ProcessingPickup += OnUpgradingPickup;
        Scp914Events.ProcessedPickup += OnUpgradedPickup;
        
        TypeExtensions.ForEachLoadedType(type =>
        {
            if (!typeof(CustomItemHandler).IsAssignableFrom(type))
                return;

            if (type == typeof(TestCustomItemHandler) && !TestCustomItemHandler.IsEnabled)
                return;

            Register(type);
        });
    }
}