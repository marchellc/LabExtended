using HarmonyLib;

using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.ShotEvents;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Patches.Functions.Items.Firearms;

public static class AmmoRemovePatches
{
    [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.ServerShoot))]
    private static bool AutomaticPrefix(AutomaticActionModule __instance, ReferenceHub primaryTarget)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return true;
        
        var ammoToFire = 0;
        
        if (__instance.OpenBolt)
        {
            ammoToFire = Mathf.Min(__instance.PrimaryAmmoContainer.AmmoStored, __instance.ChamberSize);
            
            if (!player.Toggles.HasUnlimitedAmmo)
                __instance.PrimaryAmmoContainer.ServerModifyAmmo(-ammoToFire);
        }
        else
        {
            ammoToFire = __instance.AmmoStored;

            if (!player.Toggles.HasUnlimitedAmmo)
                __instance.AmmoStored = 0;
        }
        
        __instance._serverQueuedRequests.Trigger(__instance.TimeBetweenShots);
        __instance.SendRpc(x => x != __instance.Firearm.Owner, writer =>
        {
            writer.WriteSubheader(AutomaticActionModule.MessageHeader.RpcFire);
            writer.WriteByte((byte)ammoToFire);
        });
        
        __instance.PlayFire(ammoToFire);
        
        if (!player.Toggles.HasUnlimitedAmmo)
            __instance.ServerCycleAction();

        if (!__instance.Firearm.TryGetModule<IHitregModule>(out var hitregModule))
            return false;
        
        for (var i = 0; i < ammoToFire; i++)
            hitregModule.Fire(primaryTarget, new BulletShotEvent(__instance.Firearm.ItemId, i));

        return false;
    }
}