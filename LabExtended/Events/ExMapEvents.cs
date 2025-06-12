using LabExtended.Events.Map;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// Map-specific events.
/// </summary>
public static class ExMapEvents
{
    /// <inheritdoc cref="DistributedPickupEventArgs"/>
    public static event Action<DistributedPickupEventArgs>? DistributedPickup;
    
    /// <inheritdoc cref="DistributingPickupEventArgs"/>
    public static event Action<DistributingPickupEventArgs>? DistributingPickup; 
    
    /// <inheritdoc cref="LockerSpawningPickupEventArgs"/>
    public static event Action<LockerSpawningPickupEventArgs>? LockerSpawningPickup;

    /// <inheritdoc cref="LockerSpawnedPickupEventArgs"/>
    public static event Action<LockerSpawnedPickupEventArgs>? LockerSpawnedPickup; 
    
    /// <inheritdoc cref="PocketDimensionDestroyingItemEventArgs"/>
    public static event Action<PocketDimensionDestroyingItemEventArgs>? PocketDimensionDestroyingItem; 
    
    /// <inheritdoc cref="PocketDimensionDroppingItemEventArgs"/>
    public static event Action<PocketDimensionDroppingItemEventArgs>? PocketDimensionDroppingItem; 
    
    /// <inheritdoc cref="SpawningStructureEventArgs"/>
    public static event Action<SpawningStructureEventArgs>? SpawningStructure; 
    
    /// <inheritdoc cref="TeslaGateStartedIdlingEventArgs"/>
    public static event Action<TeslaGateStartedIdlingEventArgs>? TeslaGateStartedIdling;
    
    /// <inheritdoc cref="TeslaGateStartedIdlingEventArgs"/>
    public static event Action<TeslaGateStoppedIdlingEventArgs>? TeslaGateStoppedIdling;
    
    /// <inheritdoc cref="TeslaGateTriggeringEventArgs"/>
    public static event Action<TeslaGateTriggeringEventArgs>? TeslaGateTriggering;
    
    /// <inheritdoc cref="PickupCollidedEventArgs"/>
    public static event Action<PickupCollidedEventArgs>? PickupCollided; 

    /// <summary>
    /// Executes the <see cref="DistributedPickup"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static void OnDistributedPickup(DistributedPickupEventArgs args)
        => DistributedPickup.InvokeEvent(args);

    /// <summary>
    /// Executes the <see cref="DistributingPickup"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnDistributingPickup(DistributingPickupEventArgs args)
        => DistributingPickup.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="LockerSpawningPickup"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnLockerSpawningPickup(LockerSpawningPickupEventArgs args)
        => LockerSpawningPickup.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="LockerSpawnedPickup"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static void OnLockerSpawnedPickup(LockerSpawnedPickupEventArgs args)
        => LockerSpawnedPickup.InvokeEvent(args);
    
    /// <summary>
    /// Executes the <see cref="PocketDimensionDestroyingItem"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnPocketDimensionDestroyingItem(PocketDimensionDestroyingItemEventArgs args)
        => PocketDimensionDestroyingItem.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="PocketDimensionDroppingItem"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnPocketDimensionDroppingItem(PocketDimensionDroppingItemEventArgs args)
        => PocketDimensionDroppingItem.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="SpawningStructure"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnSpawningStructure(SpawningStructureEventArgs args)
        => SpawningStructure.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="TeslaGateStartedIdling"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static void OnTeslaGateStartedIdling(TeslaGateStartedIdlingEventArgs args)
        => TeslaGateStartedIdling.InvokeEvent(args);
    
    /// <summary>
    /// Executes the <see cref="TeslaGateStoppedIdling"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static void OnTeslaGateStoppedIdling(TeslaGateStoppedIdlingEventArgs args)
        => TeslaGateStoppedIdling.InvokeEvent(args);
    
    /// <summary>
    /// Executes the <see cref="TeslaGateTriggering"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnTeslaGateTriggering(TeslaGateTriggeringEventArgs args)
        => TeslaGateTriggering.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="PickupCollided"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The event's <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnPickupCollided(PickupCollidedEventArgs args)
        => PickupCollided.InvokeBooleanEvent(args);
}