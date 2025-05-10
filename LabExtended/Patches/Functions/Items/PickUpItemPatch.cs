using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Extensions;

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

            var tracker = __instance.TargetPickup.GetTracker();
            var item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                ItemAddReason.PickedUp,
                __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

            if (item != null)
            {
                var pickedUpArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);
                
                tracker.SetItem(item, player);
                
                __instance.CheckCategoryLimitHint();
                __instance.TargetPickup.DestroySelf();

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
