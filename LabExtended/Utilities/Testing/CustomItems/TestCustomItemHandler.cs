using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;
using LabExtended.API.CustomItems.Properties;

using LabExtended.Core;
using LabExtended.Events;
using MEC;
using UnityEngine;

namespace LabExtended.Utilities.Testing.CustomItems;

/// <summary>
/// A custom item handler designed for testing.
/// </summary>
public class TestCustomItemHandler : CustomItemHandler
{
#if ENABLE_TEST_CUSTOM_ITEM
    public static bool IsEnabled = true;
#else
    public static bool IsEnabled = false;
#endif
    
    /// <inheritdoc cref="CustomItemHandler.Id"/>
    public override ushort Id => 0;

    /// <inheritdoc cref="CustomItemHandler.Name"/>
    public override string Name { get; } = "Test Custom Item";

    /// <inheritdoc cref="CustomItemHandler.Description"/>
    public override string Description { get; } = "Custom Item used for testing the API";

    /// <inheritdoc cref="CustomItemHandler.InventoryProperties"/>
    public override CustomItemInventoryProperties? InventoryProperties { get; } = new()
    {
        Type = ItemType.Coin
    };

    /// <inheritdoc cref="CustomItemHandler.PickupProperties"/>
    public override CustomItemPickupProperties? PickupProperties { get; } = new()
    {
        Type = ItemType.Coin,
        Scale = Vector3.one * 3f
    };

    /// <inheritdoc cref="CustomItemHandler.InventoryBehaviourType"/>
    public override Type InventoryBehaviourType { get; } = typeof(TestCustomItemInventoryBehaviour);

    /// <inheritdoc cref="CustomItemHandler.PickupBehaviourType"/>
    public override Type PickupBehaviourType { get; } = typeof(TestCustomItemPickupBehaviour);

    /// <inheritdoc cref="CustomItemHandler.InitializeItem"/>
    public override void InitializeItem(CustomItemInventoryBehaviour item, CustomItemPickupBehaviour? pickup = null)
    {
        base.InitializeItem(item, pickup);
        ApiLog.Debug("TestCustomItemHandler", $"Initialized item: {item.Item.ItemSerial} ({item.Item.ItemTypeId}) [{item.Player?.Nickname ?? "null"}]");
    }

    /// <inheritdoc cref="CustomItemHandler.InitializePickup"/>
    public override void InitializePickup(CustomItemPickupBehaviour pickup, CustomItemInventoryBehaviour? item = null)
    {
        base.InitializePickup(pickup, item);
        ApiLog.Debug("TestCustomItemHandler", $"Initialized pickup: {pickup.Pickup.Info.Serial} ({pickup.Pickup.Info.ItemId}) [{pickup.Player?.Nickname ?? "null"}]");
    }

    /// <inheritdoc cref="CustomItemHandler.OnRegistered"/>
    public override void OnRegistered()
    {
        base.OnRegistered();
        
        ApiLog.Debug("TestCustomItemHandler", $"Registered");

        InternalEvents.OnRoundStarted += OnStarted;
    }

    /// <inheritdoc cref="CustomItemHandler.OnUnregistered"/>
    public override void OnUnregistered()
    {
        base.OnUnregistered();
        
        ApiLog.Debug("TestCustomItemHandler", $"Unregistered");
        
        InternalEvents.OnRoundStarted -= OnStarted;
    }

    private void OnStarted()
    {
        if (IsEnabled)
        {
            MEC.Timing.CallDelayed(5f, () =>
            {
                ExPlayer.Players.ForEach(p =>
                {
                    Give(p);
                    Spawn(p.CameraTransform.position, p.CameraTransform.rotation, p);
                });
            });
        }
    }
}