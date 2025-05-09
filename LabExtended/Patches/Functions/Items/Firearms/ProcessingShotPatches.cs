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

        if (!__instance._serverQueuedRequests.TryDequeue(out var dequeued))
            return false;

        if (!__instance.Cocked)
        {
            __instance.ServerSendRejection(AutomaticActionModule.RejectionReason.NotCocked);
            return false;
        }

        if (__instance.BoltLocked)
        {
            __instance.ServerSendRejection(AutomaticActionModule.RejectionReason.BoltLocked);
            return false;
        }

        for (var i = 0; i < __instance.Firearm.Modules.Length; i++)
        {
            if (__instance.Firearm.Modules[i] is not IBusyIndicatorModule { IsBusy: true } busyIndicatorModule
                || busyIndicatorModule == __instance)
                continue;
            
            __instance.ServerSendRejection(AutomaticActionModule.RejectionReason.ModuleBusy, (busyIndicatorModule as ModuleBase).SyncId);
            return false;
        }

        ExPlayer? targetPlayer = dequeued.BacktrackData.HasPrimaryTarget
            ? ExPlayer.Get(dequeued.BacktrackData.PrimaryTargetHub)
            : null;

        Vector3? targetPosition = dequeued.BacktrackData.HasPrimaryTarget
            ? dequeued.BacktrackData.PrimaryTargetRelativePosition.Position
            : null;

        var customFirearm = __instance.Firearm.GetTracker().CustomItem as CustomFirearmInstance;

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

    [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessStartCmd))]
    public static bool DisruptorPrefix(DisruptorActionModule __instance, bool ads)
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

        var customFirearm = __instance.Firearm.GetTracker().CustomItem as CustomFirearmInstance;

        if (customFirearm is not null && !customFirearm.OnProcessingShot(null, null))
        {
            __instance.SendRpc(x => x.WriteSubheader(DisruptorActionModule.MessageType.RpcStopFiring));
            return false;
        }

        __instance.SendRpc(x =>
        {
            x.WriteSubheader(DisruptorActionModule.MessageType.RpcStartFiring);
            x.WriteBool(ads);
            x.WriteBool(__instance._magModule.AmmoStored == 1);
        });
        
        if (shootingEventArgs.Player is not ExPlayer player
            || !player.Toggles.HasUnlimitedAmmo)
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
        
        var customFirearm = __instance.Firearm.GetTracker().CustomItem as CustomFirearmInstance;

        if (customFirearm != null && !customFirearm.OnProcessingShot(targetPlayer, targetPosition))
            return false;
        
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

        var customFirearm = __instance.Firearm.GetTracker().CustomItem as CustomFirearmInstance;

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