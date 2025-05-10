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

            var pickupBehaviours = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
            
            CustomItemUtils.GetPickupBehavioursNonAlloc(__instance.TargetPickup.Info.Serial, pickupBehaviours);

            for (var i = 0; i < pickupBehaviours.Count; i++)
                pickupBehaviours[i].OnPickingUp(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
            {
                ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
                return false;
            }

            var tracker = __instance.TargetPickup.GetTracker();
            var targetBehaviour = CustomItemUtils.SelectPickupBehaviour(pickupBehaviours);

            ItemBase? item = null;

            if (targetBehaviour != null)
            {
                item = player.Inventory.AddItem(targetBehaviour.Handler.InventoryProperties.Type,
                    ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial);
            }
            else
            {
                item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                    ItemAddReason.PickedUp,
                    __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
            }

            if (item != null)
            {
                var pickedUpArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);
                
                tracker.SetItem(item, player);
                
                CustomItemUtils.ProcessPickedUp(pickupBehaviours, item, player, pickedUpArgs);
                
                __instance.CheckCategoryLimitHint();
                __instance.TargetPickup.DestroySelf();

                PlayerEvents.OnPickedUpItem(pickedUpArgs);
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
            return false;
        }
    }
}
