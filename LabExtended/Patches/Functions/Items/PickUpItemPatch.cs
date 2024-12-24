using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpItemPatch
    {
        [HookPatch(typeof(PlayerPickingUpItemArgs), true)]
        [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
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

            var pickingUpArgs = new PlayerPickingUpItemEventArgs(player.Hub, __instance.TargetPickup);

            PlayerEvents.OnPickingUpItem(pickingUpArgs);

            if (!pickingUpArgs.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpEv = new PlayerPickingUpItemArgs(player, __instance.TargetPickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, true);

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

            ItemBase item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId, ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

            if (item != null)
            {
                __instance.CheckCategoryLimitHint();

                if (pickingUpEv.DestroyPickup)
                    __instance.TargetPickup.DestroySelf();

                PlayerEvents.OnPickedUpItem(new PlayerPickedUpItemEventArgs(player.Hub, item));
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}
