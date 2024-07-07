using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Extensions;
using LabExtended.Events;

using PluginAPI.Events;
using PluginAPI.Core.Items;
using InventorySystem.Items;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Enums;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.Complete))]
    public static class PickUpArmorPatch
    {
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

            if (!EventManager.ExecuteEvent(new PlayerPickupAmmoEvent(__instance.Hub, __instance.TargetPickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            if (__instance._currentArmor != null)
            {
                __instance._currentArmor.DontRemoveExcessOnDrop = true;
                __instance.Hub.inventory.ServerDropItem(__instance._currentArmorSerial);
            }

            var pickingUpEv = new PlayerPickingUpItemArgs(player, __instance.TargetPickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, true);

            pickingUpEv.Cancellation = true;

            if (CustomItem.TryGetItem(__instance.TargetPickup, out var customItem))
            {
                customItem.IsSelected = false;
                customItem.OnPickingUp(pickingUpEv);
            }

            if (!HookRunner.RunCancellable(pickingUpEv, pickingUpEv.Cancellation))
            {
                if (pickingUpEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            ItemBase item = null;

            if (customItem != null)
            {
                if (customItem.Info.InventoryType != ItemType.None)
                    item = __instance.Hub.inventory.ServerAddItem(customItem.Info.InventoryType, customItem.Serial, __instance.TargetPickup);
            }
            else
            {
                item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
            }

            if (item != null)
            {
                if (item is BodyArmor bodyArmor)
                    BodyArmorUtils.RemoveEverythingExceedingLimits(__instance.Hub.inventory, bodyArmor);

                if (customItem != null)
                {
                    customItem.Pickup = null;
                    customItem.Item = item;

                    customItem.OnPickedUp(pickingUpEv);

                    if ((customItem.Info.ItemFlags & CustomItemFlags.SelectOnPickup) == CustomItemFlags.SelectOnPickup)
                        customItem.Select();
                }

                if (pickingUpEv.DestroyPickup)
                    __instance.TargetPickup.DestroySelf();
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}