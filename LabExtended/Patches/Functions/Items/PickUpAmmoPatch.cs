using HarmonyLib;

using Hints;

using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Searching;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using PluginAPI.Events;

namespace LabExtended.Patches.Functions.Items
{
    [HarmonyPatch(typeof(AmmoSearchCompletor), nameof(AmmoSearchCompletor.Complete))]
    public static class PickUpAmmoPatch
    {
        public static bool Prefix(AmmoSearchCompletor __instance)
        {
            if (__instance.TargetPickup is null || __instance.TargetPickup is not AmmoPickup ammoPickup)
                return false;

            var player = ExPlayer.Get(__instance.Hub);

            if (player is null)
                return true;

            if (!player.Switches.CanPickUpAmmo)
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var curAmmo = __instance.CurrentAmmo;
            var ammo = (uint)Math.Min(curAmmo + ammoPickup.SavedAmmo, __instance.MaxAmmo) - curAmmo;

            if (!EventManager.ExecuteEvent(new PlayerPickupAmmoEvent(__instance.Hub, ammoPickup)))
            {
                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            var pickUpEv = new PlayerPickingUpAmmoArgs(player, ammoPickup, __instance, __instance.Hub.searchCoordinator.SessionPipe, __instance.Hub.searchCoordinator, (ammoPickup.SavedAmmo - ammo) <= 0, ammo);

            if (!HookRunner.RunEvent(pickUpEv, true))
            {
                if (pickUpEv.DestroyPickup)
                {
                    __instance.TargetPickup.DestroySelf();
                    return false;
                }

                __instance.TargetPickup.UnlockPickup();
                return false;
            }

            ammo = pickUpEv.Amount;

            if (ammo > 0)
            {
                ammoPickup.NetworkSavedAmmo = (ushort)(ammoPickup.SavedAmmo - ammo);

                __instance.CurrentAmmo += (ushort)ammo;

                if (__instance.CurrentAmmo >= __instance.MaxAmmo)
                {
                    __instance.Hub.hints.Show(new TranslationHint(HintTranslations.MaxAmmoReached, new HintParameter[2]
                    {
                        new AmmoHintParameter((byte)__instance._ammoType),
                        new PackedULongHintParameter(__instance.MaxAmmo)
                    }, HintEffectPresets.FadeInAndOut(0.25f), 1.5f));
                }
            }

            if (pickUpEv.DestroyPickup)
                ammoPickup.DestroySelf();
            else
                ammoPickup.UnlockPickup();

            return false;
        }
    }
}