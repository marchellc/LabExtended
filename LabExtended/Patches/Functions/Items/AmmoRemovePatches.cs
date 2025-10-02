using HarmonyLib;

using InventorySystem.Items.Autosync;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;

using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using InventorySystem.Items.Firearms.ShotEvents;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Containers;

using Mirror;

using UnityEngine;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements the functionality of the <see cref="SwitchContainer.HasUnlimitedAmmo"/> toggle.
/// </summary>
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
        __instance.ServerCycleAction();

        if (!__instance.Firearm.TryGetModule<IHitregModule>(out var hitregModule))
            return false;
        
        for (var i = 0; i < ammoToFire; i++)
            hitregModule.Fire(primaryTarget, new BulletShotEvent(__instance.Firearm.ItemId, i));

        return false;
    }

    [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessStartCmd))]
    private static bool DisruptorPrefix(DisruptorActionModule __instance, bool ads)
    {
        if (__instance.IsReloading || __instance.CurFiringState != DisruptorActionModule.FiringState.None
                                   || __instance._magModule.AmmoStored == 0)
            return false;

        var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);

        PlayerEvents.OnShootingWeapon(shootingEventArgs);

        if (!shootingEventArgs.IsAllowed)
        {
            __instance.SendRpc(x => x.WriteSubheader(DisruptorActionModule.MessageType.RpcStopFiring));
            return false;
        }

        __instance.SendRpc(x =>
        {
            x.WriteSubheader(DisruptorActionModule.MessageType.RpcStartFiring);

            x.WriteBool(ads);
            x.WriteBool(__instance._magModule.AmmoStored == 1
                        && (shootingEventArgs.Player is not ExPlayer ply || !ply.Toggles.HasUnlimitedAmmo));
        });

        if (shootingEventArgs.Player is not ExPlayer player
            || !player.Toggles.HasUnlimitedAmmo)
            __instance._magModule.ServerModifyAmmo(-1);

        PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        return false;
    }

    [HarmonyPatch(typeof(DoubleActionModule), nameof(DoubleActionModule.FireLive))]
    private static bool DoubleActionPrefix(DoubleActionModule __instance, CylinderAmmoModule.Chamber chamber, NetworkReader extraData)
    {
        var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);

        PlayerEvents.OnShootingWeapon(shootingEventArgs);

        if (!shootingEventArgs.IsAllowed)
            return false;

        var shotData = new ShotBacktrackData(extraData);
        var shotEvent = new BulletShotEvent(__instance.Firearm.OwnerInventory.CurItem);

        shotData.ProcessShot(__instance.Firearm, hub => __instance._hitregModule.Fire(hub, shotEvent));

        __instance.SendRpc(t => t != __instance.Firearm.Owner && !t.isLocalPlayer,
            x => x.WriteSubheader(DoubleActionModule.MessageType.RpcFire));

        var clipOverride = __instance.Firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride);

        __instance._audioModule.PlayGunshot(__instance._fireClips[(int)clipOverride]);

        if (shootingEventArgs.Player is not ExPlayer player
            || !player.Toggles.HasUnlimitedAmmo)
            chamber.ContextState = CylinderAmmoModule.ChamberState.Discharged;

        ShotEventManager.Trigger(shotEvent);

        __instance.PlayFireAnims(FirearmAnimatorHashes.Fire);

        PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        return false;
    }
}