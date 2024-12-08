using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabExtended.API;

using LabExtended.Core.Hooking;
using LabExtended.Extensions;
using LabExtended.Events.Player;

using PluginAPI.Events;

using LabExtended.Attributes;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpArmorPatch
    {
        [HookPatch(typeof(PlayerPickupArmorEvent), true)]
        [HookPatch(typeof(PlayerPickingUpItemArgs), true)]
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

            if (!player.Switches.CanPickUpItems)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            if (!EventManager.ExecuteEvent(new PlayerPickupArmorEvent(__instance.Hub, __instance.TargetPickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            if (__instance._currentArmor != null)
            {
                __instance._currentArmor.DontRemoveExcessOnDrop = true;
                __instance.Hub.inventory.ServerDropItem(__instance._currentArmorSerial);
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
                if (item is BodyArmor bodyArmor)
                    BodyArmorUtils.RemoveEverythingExceedingLimits(__instance.Hub.inventory, bodyArmor);

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