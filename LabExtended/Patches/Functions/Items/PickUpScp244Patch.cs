using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
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

            var tracker = __instance.TargetPickup.GetTracker();
            var item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                ItemAddReason.PickedUp,
                __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

            if (item != null)
            {
                var pickedUpArgs = new PlayerPickedUpItemEventArgs(player.ReferenceHub, item);
                
                tracker.SetItem(item, player);

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