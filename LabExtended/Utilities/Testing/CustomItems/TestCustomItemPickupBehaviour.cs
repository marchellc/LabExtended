using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API.CustomItems.Behaviours;
using LabExtended.Core;

using PlayerThrowingItemEventArgs = LabExtended.Events.Player.PlayerThrowingItemEventArgs;

namespace LabExtended.Utilities.Testing.CustomItems;

public class TestCustomItemPickupBehaviour : CustomItemPickupBehaviour
{
    public override void OnEnabled()
    {
        base.OnEnabled();
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnEnabled");
    }

    public override void OnDisabled()
    {
        base.OnDisabled();
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnDisabled");
    }

    public override void OnSpawned(CustomItemInventoryBehaviour? item = null)
    {
        base.OnSpawned(item);
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnSpawned ({item?.Item?.ItemSerial ?? -1})");
    }

    public override void OnThrowing(PlayerThrowingItemEventArgs args)
    {
        base.OnThrowing(args);
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnThrowing");
    }

    public override void OnThrown(PlayerThrewItemEventArgs args)
    {
        base.OnThrown(args);
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnThrown");
    }

    public override void OnPickingUp(PlayerPickingUpItemEventArgs args)
    {
        base.OnPickingUp(args);
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnPickingUp {args.Player.Nickname}");
    }

    public override void OnPickedUp(PlayerPickedUpItemEventArgs args, CustomItemInventoryBehaviour item)
    {
        base.OnPickedUp(args, item);
        ApiLog.Debug("TestCustomItemPickupBehaviour", $"OnPickedUp ({item?.Item?.ItemSerial ?? -1})");
    }
}