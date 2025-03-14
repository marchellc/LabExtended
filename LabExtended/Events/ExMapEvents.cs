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
    
    /// <inheritdoc cref="DistributingPickupsEventArgs"/>
    public static event Action<DistributingPickupEventArgs>? DistributingPickup; 
    
    /// <inheritdoc cref="DistributingPickupsEventArgs"/>
    public static event Action<DistributingPickupsEventArgs>? DistributingPickups; 
    
    /// <inheritdoc cref="DistributingPickupsEventArgs"/>
    public static event Action<LockerFillingChamberEventArgs>? LockerFillingChamber; 
    
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

    /// <inheritdoc cref="WarheadChangingLeverEventArgs"/>
    public static event Action<WarheadChangingLeverEventArgs>? WarheadChangingLever;

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
    /// Executes the <see cref="DistributingPickups"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnDistributingPickups(DistributingPickupsEventArgs args)
        => DistributingPickups.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Executes the <see cref="LockerFillingChamber"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnLockerFillingChamber(LockerFillingChamberEventArgs args)
        => LockerFillingChamber.InvokeBooleanEvent(args);
    
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
    /// Executes the <see cref="WarheadChangingLever"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public static bool OnWarheadChangingLever(WarheadChangingLeverEventArgs args)
        => WarheadChangingLever.InvokeBooleanEvent(args);
}