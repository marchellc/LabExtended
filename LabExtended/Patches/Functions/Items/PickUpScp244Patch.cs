using HarmonyLib;

using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;

using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Extensions; 

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpScp244Patch
    {
        [HarmonyPatch(typeof(Scp244SearchCompletor), nameof(Scp244SearchCompletor.Complete))]
        public static bool Prefix(Scp244SearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return false;

            if (__instance.TargetPickup == null || __instance.TargetPickup is not Scp244DeployablePickup scp244DeployablePickup)
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

            var pickupBehaviour = CustomItemUtils.TryGetBehaviour<CustomItemPickupBehaviour>(__instance.TargetPickup.Info.Serial, out var b)
                ? b
                : null;
            
            pickupBehaviour?.OnPickingUp(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
                return false;

            var pickupType = pickupBehaviour.GetCustomValue(pb => pb.Handler.InventoryProperties.Type,
                type => type != ItemType.None, __instance.TargetPickup.Info.ItemId);
            
            var item = player.Inventory.AddItem(pickupType, ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial);

            if (item != null)
            {
                var pickedUpArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);
                
                if (__instance.TargetPickup.TryGetTracker(out var tracker))
                    tracker.SetItem(item, player);
                
                pickupBehaviour.ProcessPickedUp(item, player, pickedUpArgs);

                scp244DeployablePickup.State = Scp244State.PickedUp;

                __instance.CheckCategoryLimitHint();

                PlayerEvents.OnPickedUpItem(pickedUpArgs);
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}