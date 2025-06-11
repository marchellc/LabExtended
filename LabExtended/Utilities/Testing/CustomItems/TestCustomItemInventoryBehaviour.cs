using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API.CustomItems.Behaviours;
using LabExtended.Core;
using LabExtended.Events.Player;

namespace LabExtended.Utilities.Testing.CustomItems;

/// <summary>
/// Inventory behaviour for testing purposes.
/// </summary>
public class TestCustomItemInventoryBehaviour : CustomItemInventoryBehaviour
{
    public override void OnEnabled()
    {
        base.OnEnabled();
        ApiLog.Debug("TestCustomItemInventoryBehaviour", "OnEnabled");
    }

    public override void OnDisabled()
    {
        base.OnDisabled();
        ApiLog.Debug("TestCustomItemInventoryBehaviour", "OnDisabled");
    }

    public override void OnAdded(CustomItemPickupBehaviour? pickup = null)
    {
        base.OnAdded(pickup);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", $"OnAdded ({pickup?.Pickup?.Info.Serial ?? -1})");
    }

    public override void OnSelected(PlayerSelectedItemEventArgs args)
    {
        base.OnSelected(args);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", $"Selected ({IsSelected})");
    }

    public override void OnSelecting(PlayerSelectingItemEventArgs args)
    {
        base.OnSelecting(args);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", "Selecting");
    }

    public override void OnUnselected(PlayerSelectedItemEventArgs args)
    {
        base.OnUnselected(args);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", $"Unselected ({IsSelected})");
    }

    public override void OnUnselecting(PlayerSelectingItemEventArgs args)
    {
        base.OnUnselecting(args);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", "Unselecting");
    }

    public override void OnDropping(PlayerDroppingItemEventArgs args)
    {
        base.OnDropping(args);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", "Dropping");
    }

    public override void OnDropped(PlayerDroppedItemEventArgs args, CustomItemPickupBehaviour pickup)
    {
        base.OnDropped(args, pickup);
        ApiLog.Debug("TestCustomItemInventoryBehaviour", $"OnDropped ({pickup?.Pickup?.Info.Serial ?? -1})");
    }
}