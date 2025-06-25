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
        foreach (var handler in Handlers)
        {
            try
            {
                handler.Value.Update();
            }
            catch (Exception ex)
            {
                ApiLog.Error("Custom Item Registry", $"Could not update handler &3{handler.Key.Name}&r " +
                                                     $"(&6{handler.Value.Id}&r - &6{handler.Value.Name}&r):\n{ex}");
            }
        }
    }

    private static void OnRestart()
    {
        var behaviours = ListPool<CustomItemBehaviour>.Shared.Rent();
        
        foreach (var handler in Handlers)
        {
            behaviours.Clear();

            foreach (var invBehaviour in handler.Value.inventoryItems)
                behaviours.Add(invBehaviour.Value);
            
            foreach (var pickupBehaviour in handler.Value.pickupItems)
                behaviours.Add(pickupBehaviour.Value);
        }
        
        behaviours.ForEach(x =>
        {
            if (x is CustomItemInventoryBehaviour inventoryBehaviour)
            {
                x.Handler.DestroyItem(inventoryBehaviour);
                return;
            }
            
            x.Handler.DestroyPickup((CustomItemPickupBehaviour)x);
        });
        
        ListPool<CustomItemBehaviour>.Shared.Return(behaviours);
    }
    
    private static void OnLeaving(ExPlayer player)
    {
        var behaviours = ListPool<CustomItemBehaviour>.Shared.Rent();
        
        foreach (var handler in Handlers)
        {
            behaviours.Clear();

            foreach (var invBehaviour in handler.Value.inventoryItems)
            {
                if (invBehaviour.Value.Player is null || invBehaviour.Value.Player != player)
                    continue;
                
                behaviours.Add(invBehaviour.Value);
            }

            foreach (var pickupBehaviour in handler.Value.pickupItems)
            {
                if (pickupBehaviour.Value.Player is null || pickupBehaviour.Value.Player != player)
                    continue;
                
                behaviours.Add(pickupBehaviour.Value);
            }
        }
        
        behaviours.ForEach(x =>
        {
            if (x is CustomItemInventoryBehaviour inventoryBehaviour)
            {
                if (inventoryBehaviour.Handler.InventoryProperties.DropOnOwnerLeave)
                {
                    inventoryBehaviour.Drop();
                    return;
                }
                
                inventoryBehaviour.Destroy(true);
            }
            else if (x is CustomItemPickupBehaviour pickupBehaviour)
            {
                if (pickupBehaviour.Handler.PickupProperties.DestroyOnOwnerLeave)
                {
                    pickupBehaviour.Destroy(true);
                }
            }
        });
        
        ListPool<CustomItemBehaviour>.Shared.Return(behaviours);
    }

    private static void OnTogglingFlashlight(PlayerTogglingFlashlightEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour(args.LightItem.Serial, item => item.OnTogglingLight(args));

    private static void OnToggledFlashlight(PlayerToggledFlashlightEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour(args.LightItem.Serial, item => item.OnToggledLight(args));

    private static void OnFlippingCoin(PlayerFlippingCoinEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour(args.CoinItem.Serial, item => item.OnFlippingCoin(args));

    private static void OnFlippedCoin(PlayerFlippedCoinEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour(args.CoinItem.Serial, item => item.OnFlippedCoin(args));

    private static void OnCollided(PickupCollidedEventArgs args)
        => CustomItemUtils.ForEachPickupBehaviour(args.Pickup.Info.Serial, pickup => pickup.OnCollided(args));

    private static void OnDisarming(PlayerCuffingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnDisarming(args));
        }
    }

    private static void OnDisarmed(PlayerCuffedEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnDisarmed(args));
        }
    }

    private static void OnEscaping(PlayerEscapingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnEscaping(args));
        }
    }

    private static void OnEscaped(PlayerEscapedEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnEscaped(args));
        }
    }

    private static void OnHurting(PlayerHurtingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnHurting(args));
        }
    }

    private static void OnHurt(PlayerHurtEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnHurt(args));
        }
    }

    private static void OnDying(PlayerDyingEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnDying(args));
        }
    }

    private static void OnDied(PlayerDeathEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnDied(args));
        }
    }

    private static void OnChangingRole(PlayerChangingRoleEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnChangingRole(args));
        }
    }

    private static void OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        if (args.Player is ExPlayer player && player.Inventory.ItemCount > 0)
        {
            player.Inventory.Items.ForEachInventoryBehaviour(item => item.OnChangedRole(args));
        }
    }
    
    private static void OnUpgradingItem(Scp914ProcessingInventoryItemEventArgs args)
        => CustomItemUtils.ForEachInventoryBehaviour(args.Item.Serial, item => item.OnUpgrading(args));

    private static void OnUpgradingPickup(Scp914ProcessingPickupEventArgs args)
        => CustomItemUtils.ForEachPickupBehaviour(args.Pickup.Serial, item => item.OnUpgrading(args));

    private static void OnUpgradedItem(Scp914ProcessedInventoryItemEventArgs args)
    {
        if (args.Item?.Base != null)
        {
            var behaviours = ListPool<CustomItemInventoryBehaviour>.Shared.Rent();

            CustomItemUtils.GetInventoryBehavioursNonAlloc(args.Item.Serial, behaviours);

            for (var i = 0; i < behaviours.Count; i++)
            {
                var behaviour = behaviours[i];

                behaviour.Item = args.Item.Base;
                behaviour.IsSelected = args.Item.IsEquipped;

                if (args.Item.CurrentOwner is ExPlayer owner)
                    behaviour.Player = owner;

                behaviour.OnUpgraded(args);
            }

            ListPool<CustomItemInventoryBehaviour>.Shared.Return(behaviours);
        }
    }

    private static void OnUpgradedPickup(Scp914ProcessedPickupEventArgs args)
    {
        if (args.Pickup?.Base != null)
        {
            var behaviours = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
            
            CustomItemUtils.GetPickupBehavioursNonAlloc(args.Pickup.Serial, behaviours);

            for (var i = 0; i < behaviours.Count; i++)
            {
                var behaviour = behaviours[i];
                
                behaviour.Pickup = args.Pickup.Base;
                
                if (args.Pickup.LastOwner is ExPlayer player)
                    behaviour.Player = player;
                
                behaviour.OnUpgraded(args);
            }
            
            ListPool<CustomItemPickupBehaviour>.Shared.Return(behaviours);
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