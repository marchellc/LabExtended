using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Extensions;
using LabExtended.Events;

using PluginAPI.Events;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(Scp330SearchCompletor), nameof(Scp330SearchCompletor.Complete))]
    public static class PickUpScp330Patch
    {
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

            if (!EventManager.ExecuteEvent(new PlayerPickupScp330Event(__instance.Hub, scp330Pickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickingUpItemEv = new PlayerPickingUpItemArgs(player, scp330Pickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, scp330Pickup.StoredCandies.Count - (Scp330Bag.TryGetBag(player.Hub, out var playerBag) ? 6 - playerBag.Candies.Count : 0) <= 0);

            if (!HookRunner.RunCancellable(pickingUpItemEv, true))
            {
                if (pickingUpItemEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            Scp330Bag.ServerProcessPickup(__instance.Hub, scp330Pickup, out var bag);

            if (pickingUpItemEv.DestroyPickup)
            {
                scp330Pickup.DestroySelf();
                return false;
            }

            return false;
        }
    }
}
