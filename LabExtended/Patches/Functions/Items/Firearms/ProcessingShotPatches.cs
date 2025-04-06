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
using LabExtended.API.CustomFirearms;
using LabExtended.API.CustomItems;

using LabExtended.Extensions;
using Mirror;
using UnityEngine;

namespace LabExtended.Patches.Functions.Items.Firearms;

/// <summary>
/// Patches used to trigger the <see cref="API.CustomFirearms.CustomFirearmInstance.OnProcessingShot"/> method.
/// </summary>
public static class ProcessingShotPatches
{
    [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.UpdateServer))]
    public static bool AutomaticPrefix(AutomaticActionModule __instance)
    {
        __instance._serverQueuedRequests.Update();

        if (__instance.Firearm.AnyModuleBusy(__instance)
            || !__instance._serverQueuedRequests.TryDequeue(out var dequeued)
            || !__instance.Cocked
            || !__instance.BoltLocked)
            return false;

        ExPlayer? targetPlayer = dequeued.BacktrackData.HasPrimaryTarget
            ? ExPlayer.Get(dequeued.BacktrackData.PrimaryTargetHub)
            : null;

        Vector3? targetPosition = dequeued.BacktrackData.HasPrimaryTarget
            ? dequeued.BacktrackData.PrimaryTargetRelativePosition.Position
            : null;

        var customFirearm = CustomItemManager.InventoryItems.GetValue<CustomFirearmInstance>(__instance.Firearm);

        if (__instance.AmmoStored > 0 || (__instance.OpenBolt && __instance.PrimaryAmmoContainer.AmmoStored > 0))
        {
            var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
            
            PlayerEvents.OnShootingWeapon(shootingEventArgs);

            if (!shootingEventArgs.IsAllowed)
                return false;

            if (customFirearm != null && !customFirearm.OnProcessingShot(targetPlayer, targetPosition))
                return false;
            
            dequeued.BacktrackData.ProcessShot(__instance.Firearm, __instance.ServerShoot);
            
            PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm)); 
            
            customFirearm?.OnProcessedShot(targetPlayer, targetPosition);
        }
        else
        {
            var dryFiringEventArgs = new PlayerDryFiringWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
            
            PlayerEvents.OnDryFiringWeapon(dryFiringEventArgs);

            if (!dryFiringEventArgs.IsAllowed || (customFirearm != null && !customFirearm.OnDryFiring()))
                return false;

            __instance.Cocked = false;
            
            __instance.PlayDryFire();
            __instance.ServerResync();

            __instance.SendRpc(x => x.WriteSubheader(AutomaticActionModule.MessageHeader.RpcDryFire));
            
            PlayerEvents.OnDryFiredWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
            
            customFirearm?.OnDryFired();
        }
        
        __instance.ServerSendResponse(dequeued);
        return false;
    }

    [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessCmd))]
    public static bool DisruptorPrefix(DisruptorActionModule __instance, bool ads)
    {
        if (__instance.IsReloading || __instance.CurFiringState != DisruptorActionModule.FiringState.None
                                   || __instance._magModule.AmmoStored == 0)
            return false;

        var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
        
        PlayerEvents.OnShootingWeapon(shootingEventArgs);

        if (!shootingEventArgs.IsAllowed)
            return false;

        var customFirearm = CustomItemManager.InventoryItems.GetValue<CustomFirearmInstance>(__instance.Firearm);

        if (customFirearm is not null && !customFirearm.OnProcessingShot(null, null))
            return false;
        
        __instance.SendRpc(x =>
        {
            x.WriteSubheader(DisruptorActionModule.MessageType.RpcStartFiring);
            x.WriteBool(__instance._magModule.AmmoStored == 1);
            x.WriteBool(ads);
        });
        
        __instance._magModule.ServerModifyAmmo(-1);
        
        customFirearm?.OnProcessedShot(null, null);
        
        PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        return false;
    }

    [HarmonyPatch(typeof(DoubleActionModule), nameof(DoubleActionModule.FireLive))]
    public static bool DoubleActionPrefix(DoubleActionModule __instance, CylinderAmmoModule.Chamber chamber, NetworkReader extraData)
    {
        var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
        
        PlayerEvents.OnShootingWeapon(shootingEventArgs);

        if (!shootingEventArgs.IsAllowed)
            return false;

        var shotData = new ShotBacktrackData(extraData);
        var shotEvent = new BulletShotEvent(__instance.Firearm.OwnerInventory.CurItem);

        ExPlayer? targetPlayer = shotData.HasPrimaryTarget ? ExPlayer.Get(shotData.PrimaryTargetHub) : null;
        Vector3? targetPosition = shotData.HasPrimaryTarget ? shotData.PrimaryTargetRelativePosition.Position : null;
        
        var customFirearm = CustomItemManager.InventoryItems.GetValue<CustomFirearmInstance>(__instance.Firearm);

        if (customFirearm != null && !customFirearm.OnProcessingShot(targetPlayer, targetPosition))
            return false;
        
        shotData.ProcessShot(__instance.Firearm, hub => __instance._hitregModule.Fire(hub, shotEvent));
        
        __instance.SendRpc(x => x.WriteSubheader(DoubleActionModule.MessageType.RpcFire));

        var clipOverride = __instance.Firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride);
        
        __instance._audioModule.PlayGunshot(__instance._fireClips[(int)clipOverride]);

        chamber.ContextState = CylinderAmmoModule.ChamberState.Discharged;
        
        ShotEventManager.Trigger(shotEvent);
        
        __instance.PlayFireAnims(FirearmAnimatorHashes.Fire);
        
        PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        
        customFirearm?.OnProcessedShot(targetPlayer, targetPosition);
        return false;
    }

    [HarmonyPatch(typeof(PumpActionModule), nameof(PumpActionModule.UpdateServer))]
    public static bool PumpActionPrefix(PumpActionModule __instance)
    {
        __instance._serverQueuedShots.Update();

        if (__instance.Firearm.AnyModuleBusy(__instance))
            return false;

        if (!__instance.PumpIdle)
            return false;

        if (!__instance._serverQueuedShots.TryDequeue(out var dequeued))
            return false;

        var shootingEventArgs = new PlayerShootingWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
        
        PlayerEvents.OnShootingWeapon(shootingEventArgs);

        if (!shootingEventArgs.IsAllowed)
            return false;

        var customFirearm = CustomItemManager.InventoryItems.GetValue<CustomFirearmInstance>(__instance.Firearm);

        ExPlayer? targetPlayer = dequeued.HasPrimaryTarget ? ExPlayer.Get(dequeued.PrimaryTargetHub) : null;
        Vector3? targetPosition = dequeued.HasPrimaryTarget ? dequeued.PrimaryTargetRelativePosition.Position : null;

        if (customFirearm != null && !customFirearm.OnProcessingShot(targetPlayer, targetPosition))
            return false;
        
        dequeued.ProcessShot(__instance.Firearm, __instance.ServerProcessShot);
        
        customFirearm?.OnProcessedShot(targetPlayer, targetPosition);
        
        PlayerEvents.OnShotWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        return false;
    }
}