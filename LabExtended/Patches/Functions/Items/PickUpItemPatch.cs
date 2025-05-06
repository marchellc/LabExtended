using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;

using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpItemPatch
    {
        [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
        public static bool Prefix(ItemSearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return false;

            if (__instance.TargetPickup is null)
                return false;

            if (!player.Toggles.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpArgs = new PlayerPickingUpItemEventArgs(player.ReferenceHub, __instance.TargetPickup);

            PlayerEvents.OnPickingUpItem(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            CustomItemManager.PickupItems.TryGetValue(__instance.TargetPickup, out var customItemInstance);

            ItemBase item = null;
            
            if (customItemInstance != null)
            {
                if (!customItemInstance.OnPickingUp(player))
                {
                    __instance.TargetPickup.UnlockPickup();
                    return false;
                }

                if (customItemInstance.CustomData.InventoryType is ItemType.None)
                {
                    __instance.TargetPickup.UnlockPickup();
                    return false;
                }

                item = __instance.Hub.inventory.ServerAddItem(customItemInstance.CustomData.InventoryType,
                    ItemAddReason.PickedUp, customItemInstance.ItemSerial, __instance.TargetPickup);
            }
            else
            {
                item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                    ItemAddReason.PickedUp,
                    __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
            }

            if (item != null)
            {
                if (ItemTracker.Trackers.TryGetValue(item.ItemSerial, out var tracker))
                    tracker.SetItem(item, player);
                
                if (customItemInstance != null)
                {
                    customItemInstance.Item = item;
                    
                    customItemInstance.Pickup = null;
                    customItemInstance.OnPickedUp();

                    CustomItemManager.PickupItems.Remove(__instance.TargetPickup);
                    CustomItemManager.InventoryItems.Add(item, customItemInstance);
                    
                    player.customItems.Add(item, customItemInstance);
                }
                
                __instance.CheckCategoryLimitHint();
                __instance.TargetPickup.DestroySelf();

                PlayerEvents.OnPickedUpItem(new PlayerPickedUpItemEventArgs(player.ReferenceHub, item));
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}
