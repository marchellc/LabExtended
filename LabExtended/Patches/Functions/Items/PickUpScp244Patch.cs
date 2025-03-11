using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpScp244Patch
    {
        [HookPatch(typeof(PlayerPickingUpItemArgs), true)]
        [HarmonyPatch(typeof(Scp244SearchCompletor), nameof(Scp244SearchCompletor.Complete))]
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

            var pickingUpEv = new PlayerPickingUpItemArgs(player, scp244DeployablePickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, false);

            if (!HookRunner.RunEvent(pickingUpEv, true))
            {
                if (pickingUpEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            CustomItemManager.PickupItems.TryGetValue(scp244DeployablePickup, out var customItemInstance);

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

                    CustomItemManager.PickupItems.Remove(scp244DeployablePickup);
                    CustomItemManager.InventoryItems.Add(item, customItemInstance);
                    
                    player.customItems.Add(item, customItemInstance);
                }
                
                scp244DeployablePickup.State = Scp244State.PickedUp;

                __instance.CheckCategoryLimitHint();

                if (pickingUpEv.DestroyPickup)
                {
                    scp244DeployablePickup.State = Scp244State.Destroyed;
                    scp244DeployablePickup.DestroySelf();
                }

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