using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Enums;

using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(Scp244SearchCompletor), nameof(Scp244SearchCompletor.Complete))]
    public static class PickUpScp244Patch
    {
        public static bool Prefix(Scp244SearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return false;

            if (__instance.TargetPickup is null || __instance.TargetPickup is not Scp244DeployablePickup scp244DeployablePickup)
            {
                __instance.TargetPickup?.UnlockPickup();
                return false;
            }

            if (!player.Switches.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpEv = new PlayerPickingUpItemArgs(player, scp244DeployablePickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, false);

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
                scp244DeployablePickup.State = Scp244State.PickedUp;

                __instance.CheckCategoryLimitHint();

                if (customItem != null)
                {
                    customItem.Pickup = null;
                    customItem.Item = item;

                    customItem.OnPickedUp(pickingUpEv);

                    if ((customItem.Info.ItemFlags & CustomItemFlags.SelectOnPickup) == CustomItemFlags.SelectOnPickup)
                        customItem.Select();
                }

                if (pickingUpEv.DestroyPickup)
                {
                    scp244DeployablePickup.State = Scp244State.Destroyed;
                    scp244DeployablePickup.DestroySelf();
                }
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}