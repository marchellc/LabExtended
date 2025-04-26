using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;

using LabExtended.Extensions;

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

            CustomItemManager.PickupItems.TryGetValue(__instance.TargetPickup, out var customItemInstance);

            if (__instance._currentArmor != null)
            {
                var isThrow = false;

                if (CustomItemManager.InventoryItems.TryGetValue(__instance._currentArmor, out var armorItemInstance)
                    && !armorItemInstance.OnDropping(ref isThrow))
                {
                    __instance.TargetPickup.UnlockPickup();
                    return false;
                }
                
                var pickup = __instance.Hub.inventory.ServerDropItem(__instance._currentArmor.ItemSerial);

                if (armorItemInstance != null)
                {
                    armorItemInstance.Item = null;
                    
                    armorItemInstance.Pickup = pickup;
                    armorItemInstance.OnDropped(false);
                }
            }

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
                if (customItemInstance != null)
                {
                    customItemInstance.Item = item;
                    
                    customItemInstance.Pickup = null;
                    customItemInstance.OnPickedUp();

                    CustomItemManager.PickupItems.Remove(__instance.TargetPickup);
                    CustomItemManager.InventoryItems.Add(item, customItemInstance);
                    
                    player.customItems.Add(item, customItemInstance);
                }
                
                BodyArmorUtils.SetPlayerDirty(__instance.Hub);
                
                __instance.TargetPickup.DestroySelf();

                if (item is BodyArmor bodyArmor)
                    PlayerEvents.OnPickedUpArmor(new PlayerPickedUpArmorEventArgs(player.ReferenceHub, bodyArmor));
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}