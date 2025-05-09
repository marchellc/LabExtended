using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities;
using PlayerDroppingItemEventArgs = LabExtended.Events.Player.PlayerDroppingItemEventArgs;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpArmorPatch
    {
        [HarmonyPatch(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.Complete))]
        public static bool Prefix(ArmorSearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return false;

            if (__instance.TargetPickup is null)
            {
                __instance.TargetPickup?.UnlockPickup();
                return false;
            }

            if (!player.Toggles.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpArgs = new PlayerPickingUpArmorEventArgs(player.ReferenceHub, __instance.TargetPickup as BodyArmorPickup);

            PlayerEvents.OnPickingUpArmor(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpItemArgs = new PlayerPickingUpItemEventArgs(player.ReferenceHub, __instance.TargetPickup);

            if (!pickingUpItemArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var tracker = __instance.TargetPickup.GetTracker();

            if (__instance._currentArmor != null)
            {
                var droppingArgs = new PlayerDroppingItemEventArgs(player, __instance._currentArmor, false);

                if (!ExPlayerEvents.OnDroppingItem(droppingArgs))
                    return false;
                
                var armorTracker = __instance._currentArmor.GetTracker();

                if (armorTracker.CustomItem != null)
                {
                    armorTracker.CustomItem.OnDropping(droppingArgs);

                    if (!droppingArgs.IsAllowed)
                        return false;
                }
                
                var pickup = __instance.Hub.inventory.ServerDropItem(__instance._currentArmor.ItemSerial);
                var droppedArgs = new PlayerDroppedItemEventArgs(player.ReferenceHub, pickup);
                
                PlayerEvents.OnDroppedItem(droppedArgs);
                
                armorTracker.CustomItem?.OnDropped(droppedArgs);
            }

            ItemBase item = null;
            
            if (tracker?.CustomItem != null)
            {
                tracker.CustomItem.OnPickingUp(pickingUpItemArgs);

                if (!pickingUpItemArgs.IsAllowed || tracker.CustomItem.CustomData.InventoryType is ItemType.None)
                    return false;

                item = __instance.Hub.inventory.ServerAddItem(tracker.CustomItem.CustomData.InventoryType,
                    ItemAddReason.PickedUp, tracker.CustomItem.ItemSerial, __instance.TargetPickup);
            }
            else
            {
                item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                    ItemAddReason.PickedUp,
                    __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
            }

            if (item != null)
            {
                var pickedUpItemArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);
                
                tracker.SetItem(item, player);
                tracker.CustomItem?.OnPickedUp(pickedUpItemArgs);
                
                BodyArmorUtils.SetPlayerDirty(__instance.Hub);
                
                __instance.TargetPickup.DestroySelf();

                if (item is BodyArmor bodyArmor)
                    PlayerEvents.OnPickedUpArmor(new PlayerPickedUpArmorEventArgs(player.ReferenceHub, bodyArmor));
                
                PlayerEvents.OnPickedUpItem(pickedUpItemArgs);
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}