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

        var customItems = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
        
        CustomItemUtils.GetPickupBehavioursNonAlloc(__instance.TargetPickup.Info.Serial, customItems);

        if (!pickingUpArgs.IsAllowed)
        {
            ListPool<CustomItemPickupBehaviour>.Shared.Return(customItems);
            return false;
        }

        var targetBehaviour = CustomItemUtils.SelectPickupBehaviour(customItems);
        var pickupTracker = __instance.TargetPickup.GetTracker();
        
        ItemBase? item = null;

        if (targetBehaviour != null)
            item = __instance.Hub.inventory.ServerAddItem(targetBehaviour.Handler.InventoryProperties.Type,
                ItemAddReason.PickedUp,
                __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
        else
            item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

        var pickedUpArgs = new PlayerPickedUpItemEventArgs(__instance.Hub, item);
        
        CustomItemUtils.ProcessPickedUp(customItems, item, player, pickedUpArgs);
        
        pickupTracker.SetItem(item, player);
        
        __instance.TargetPickup.DestroySelf();
        __instance.CheckCategoryLimitHint();

        PlayerEvents.OnPickedUpItem(pickedUpArgs);
        
        ListPool<CustomItemPickupBehaviour>.Shared.Return(customItems);
        return false;
    }
}
