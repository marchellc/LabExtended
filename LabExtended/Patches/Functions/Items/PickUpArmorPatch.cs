using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Events;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

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

            var pickingUpArgs =
                new PlayerPickingUpArmorEventArgs(player.ReferenceHub, __instance.TargetPickup as BodyArmorPickup);

            PlayerEvents.OnPickingUpArmor(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpItemArgs = new PlayerPickingUpItemEventArgs(player.ReferenceHub, __instance.TargetPickup);

            PlayerEvents.OnPickingUpItem(pickingUpItemArgs);

            if (!pickingUpItemArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var tracker = __instance.TargetPickup.GetTracker();

            var pickupBehaviours = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
            var currentBehaviours = ListPool<CustomItemInventoryBehaviour>.Shared.Rent();

            CustomItemUtils.GetPickupBehavioursNonAlloc(__instance.TargetPickup.Info.Serial, pickupBehaviours);

            for (var i = 0; i < pickupBehaviours.Count; i++)
                pickupBehaviours[i].OnPickingUp(pickingUpItemArgs);

            if (!pickingUpItemArgs.IsAllowed)
            {
                ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
                ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);

                return false;
            }

            if (__instance._currentArmor != null)
            {
                var droppingArgs = new PlayerDroppingItemEventArgs(player, __instance._currentArmor, false);

                if (!ExPlayerEvents.OnDroppingItem(droppingArgs))
                {
                    ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
                    ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);

                    return false;
                }

                CustomItemUtils.GetInventoryBehavioursNonAlloc(__instance._currentArmor.ItemSerial, currentBehaviours);

                for (var i = 0; i < currentBehaviours.Count; i++)
                    currentBehaviours[i].OnDropping(droppingArgs);

                if (!droppingArgs.IsAllowed)
                {
                    ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
                    ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);

                    return false;
                }

                var armorTracker = __instance._currentArmor.GetTracker();

                var pickup = __instance.Hub.inventory.ServerDropItem(__instance._currentArmor.ItemSerial);
                var droppedArgs = new PlayerDroppedItemEventArgs(player.ReferenceHub, pickup);

                PlayerEvents.OnDroppedItem(droppedArgs);

                armorTracker.SetPickup(pickup, player);
                
                CustomItemUtils.ProcessDropped(currentBehaviours, pickup, player, droppedArgs);
            }

            ItemBase? item = null;

            var targetBehaviour = CustomItemUtils.SelectPickupBehaviour(pickupBehaviours);

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
                var pickedUpItemArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);

                tracker.SetItem(item, player);
                
                CustomItemUtils.ProcessPickedUp(pickupBehaviours, item, player, pickedUpItemArgs);

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

            ListPool<CustomItemPickupBehaviour>.Shared.Return(pickupBehaviours);
            ListPool<CustomItemInventoryBehaviour>.Shared.Return(currentBehaviours);

            return false;
        }
    }
}