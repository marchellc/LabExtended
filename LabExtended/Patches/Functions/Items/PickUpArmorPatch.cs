using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Extensions;
using LabExtended.Events;

using PluginAPI.Events;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.Complete))]
    public static class PickUpArmorPatch
    {
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

            if (!EventManager.ExecuteEvent(new PlayerPickupAmmoEvent(__instance.Hub, __instance.TargetPickup)))
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

            if (!HookRunner.RunCancellable(pickingUpEv, true))
            {
                if (pickingUpEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var armor = (BodyArmor)__instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

            BodyArmorUtils.RemoveEverythingExceedingLimits(__instance.Hub.inventory, armor);

            if (pickingUpEv.DestroyPickup)
                __instance.TargetPickup.DestroySelf();

            return false;
        }
    }
}