using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements toggle functionality and custom items API.
/// </summary>
public static class PickUpItemPatch
{
    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
    private static bool Prefix(ItemSearchCompletor __instance)
    {
        if (!ExPlayer.TryGet(__instance.Hub, out var player) || !player.Toggles.CanPickUpItems)
            return false;
        
        PlayerEvents.OnSearchedPickup(new(__instance.Hub, __instance.TargetPickup));

        var pickingUpArgs = new PlayerPickingUpItemEventArgs(__instance.Hub, __instance.TargetPickup);

        PlayerEvents.OnPickingUpItem(pickingUpArgs);

        var pickupBehaviour =
            CustomItemUtils.GetBehaviour<CustomItemPickupBehaviour>(__instance.TargetPickup.Info.Serial);
        
        pickupBehaviour?.OnPickingUp(pickingUpArgs);

        if (!pickingUpArgs.IsAllowed)
            return false;

        var itemType = pickupBehaviour.GetCustomValue(pb => pb.Handler.InventoryProperties.Type,
            type => type != ItemType.None, __instance.TargetItemType);
        var item = __instance.Hub.inventory.ServerAddItem(itemType, ItemAddReason.PickedUp,
            __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

        var pickedUpArgs = new PlayerPickedUpItemEventArgs(__instance.Hub, item);
        
        pickupBehaviour.ProcessPickedUp(item, player, pickedUpArgs);
        
        if (__instance.TargetPickup.TryGetTracker(out var tracker))
            tracker.SetItem(item, player);
        
        __instance.TargetPickup.DestroySelf();
        __instance.CheckCategoryLimitHint();

        PlayerEvents.OnPickedUpItem(pickedUpArgs);
        return false;
    }
}
