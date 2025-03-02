using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;
using InventorySystem.Items;

using InventorySystem.Searching;
using InventorySystem;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Items.Candies;

using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using LabExtended.Attributes;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpScp330Patch
    {
        [HookPatch(typeof(PlayerPickingUpItemArgs), true)]
        [HarmonyPatch(typeof(Scp330SearchCompletor), nameof(Scp330SearchCompletor.Complete))]
        public static bool Prefix(Scp330SearchCompletor __instance)
        {
            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            if (__instance.TargetPickup is null || __instance.TargetPickup is not Scp330Pickup scp330Pickup)
            {
                __instance.TargetPickup?.UnlockPickup();
                return false;
            }

            if (!player.Switches.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var scp330Args = new PlayerPickingUpScp330EventArgs(player.Hub, scp330Pickup);

            PlayerEvents.OnPickingUpScp330(scp330Args);

            if (!scp330Args.IsAllowed)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpItemEv = new PlayerPickingUpItemArgs(player, scp330Pickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, false);

            if (!HookRunner.RunEvent(pickingUpItemEv, true))
            {
                if (pickingUpItemEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            if (!Scp330Bag.TryGetBag(__instance.Hub, out var bag))
            {
                var bagItem = __instance.Hub.inventory.ServerAddItem(ItemType.SCP330, ItemAddReason.PickedUp);

                if (bagItem is null)
                {
                    __instance.TargetPickup.UnlockPickup();
                    return false;
                }

                bag = (Scp330Bag)bagItem;
            }

            player.Inventory._bag ??= new CandyBag(bag, player);

            while (scp330Pickup.StoredCandies.Count > 0 && bag.TryAddSpecific(scp330Pickup.StoredCandies[0]) && ExServer.IsRunning)
                scp330Pickup.StoredCandies.RemoveAt(0);

            if (bag.AcquisitionAlreadyReceived)
                bag.ServerRefreshBag();

            PlayerEvents.OnPickedUpScp330(new PlayerPickedUpScp330EventArgs(player.Hub, scp330Pickup, bag));

            if (pickingUpItemEv.DestroyPickup || scp330Pickup.StoredCandies.Count == 0)
            {
                scp330Pickup.DestroySelf();
                return false;
            }
            else
            {
                scp330Pickup.UnlockPickup();
                return false;
            }
        }
    }
}
