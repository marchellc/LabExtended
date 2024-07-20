using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Enums;

using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using PluginAPI.Events;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
    public static class PickUpItemPatch
    {
        public static bool Prefix(ItemSearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return false;

            if (__instance.TargetPickup is null)
                return false;

            if (!player.Switches.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            if (!EventManager.ExecuteEvent(new PlayerSearchedPickupEvent(__instance.Hub, __instance.TargetPickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpEv = new PlayerPickingUpItemArgs(player, __instance.TargetPickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, true);

            pickingUpEv.IsAllowed = true;

            if (CustomItem.TryGetItem(__instance.TargetPickup, out var customItem))
            {
                customItem.IsSelected = false;
                customItem.OnPickingUp(pickingUpEv);
            }

            if (!HookRunner.RunCancellable(pickingUpEv, pickingUpEv.IsAllowed))
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
                __instance.CheckCategoryLimitHint();

                if (customItem != null)
                {
                    customItem.Pickup = null;
                    customItem.Item = item;

                    customItem.SetupItem();
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
