using HarmonyLib;

using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items
{
    public static class PickUpAmmoPatch
    {
        [HarmonyPatch(typeof(AmmoSearchCompletor), nameof(AmmoSearchCompletor.Complete))]
        public static bool Prefix(AmmoSearchCompletor __instance)
        {
            if (__instance.TargetPickup != null && ExPlayer.TryGet(__instance.Hub, out var player)
                                                && !player.Toggles.CanPickUpAmmo)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            return true;
        }
    }
}