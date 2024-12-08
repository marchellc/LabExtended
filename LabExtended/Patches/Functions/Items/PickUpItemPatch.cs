using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using PluginAPI.Events;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpItemPatch
    {
        [HookPatch(typeof(PlayerSearchedPickupEvent), true)]
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

            if (!EventManager.ExecuteEvent(new PlayerSearchedPickupEvent(__instance.Hub, __instance.TargetPickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpEv = new PlayerPickingUpItemArgs(player, __instance.TargetPickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, true);

            pickingUpEv.IsAllowed = true;

            if (!HookRunner.RunEvent(pickingUpEv, pickingUpEv.IsAllowed))
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
            }
            else
            {
                __instance.TargetPickup.UnlockPickup();
            }

            return false;
        }
    }
}
