using CameraShaking;

using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabExtended.Utilities.Generation;
using MEC;

namespace LabExtended.Utilities;

public static class FirearmUtils
{
    #region Attachments
    public static bool HasAttachment(this Firearm firearm, AttachmentName attachmentName)
    {
        if (!firearm.TryGetAttachment(attachmentName, out var attachment))
            return false;

        return attachment.IsEnabled;
    }
    
    public static void ToggleAttachment(this Firearm firearm, AttachmentName attachmentName)
    {
        if (!firearm.TryGetAttachment(attachmentName, out var attachment))
            return;

        attachment.IsEnabled = !attachment.IsEnabled;
        firearm.SyncAttachments();
    }

    public static bool ToggleAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
    {
        var anyToggled = false;

        foreach (var attachmentName in attachmentNames)
        {
            if (!firearm.TryGetAttachment(attachmentName, out var attachment))
                continue;

            attachment.IsEnabled = !attachment.IsEnabled;
            anyToggled = true;
        }

        if (anyToggled)
            firearm.SyncAttachments();

        return anyToggled;
    }

    public static bool SetAttachment(this Firearm firearm, AttachmentName attachmentName, bool enabled)
    {
        if (!firearm.TryGetAttachment(attachmentName, out var attachment))
            return false;

        if (attachment.IsEnabled == enabled)
            return false;

        attachment.IsEnabled = enabled;
        
        firearm.SyncAttachments();
        return true;
    }

    public static bool SetAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames, bool enabled)
    {
        var anyChanged = false;

        foreach (var attachmentName in attachmentNames)
        {
            if (!firearm.TryGetAttachment(attachmentName, out var attachment))
                continue;

            if (attachment.IsEnabled == enabled)
                continue;

            attachment.IsEnabled = enabled;
            anyChanged = true;
        }

        if (anyChanged)
            firearm.SyncAttachments();
        
        return anyChanged;
    }

    public static void SetRandomAttachments(this Firearm firearm)
        => firearm.ApplyAttachmentsCode(AttachmentsUtils.GetRandomAttachmentsCode(firearm.ItemTypeId), false);

    public static bool SetPreferredAttachments(this Firearm firearm)
    {
        if (firearm.Owner is null)
            return false;

        if (!AttachmentsServerHandler.PlayerPreferences.TryGetValue(firearm.Owner, out var preferences)
            || !preferences.TryGetValue(firearm.ItemTypeId, out var preferenceCode))
            return false;

        firearm.ApplyAttachmentsCode(preferenceCode, false);
        return true;
    }
    
    public static IEnumerable<AttachmentName> GetAllAttachments(this Firearm firearm)
        => firearm.Attachments.Select(x => x.Name);
    
    public static IEnumerable<AttachmentName> GetEnabledAttachmentNamess(this Firearm firearm)
        => firearm.Attachments.Where(x => x.IsEnabled).Select(x => x.Name);

    public static IEnumerable<Attachment> GetEnabledAttachments(this Firearm firearm)
        => firearm.Attachments.Where(x => x.IsEnabled);
    
    public static bool HasAllAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => attachmentNames.All(x => firearm.HasAttachment(x));

    public static bool HasAllAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => attachmentNames.All(x => firearm.HasAttachment(x));
    
    public static bool HasAnyAttachment(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => attachmentNames.Any(x => firearm.HasAttachment(x));

    public static bool HasAnyAttachment(this Firearm firearm, params AttachmentName[] attachmentNames)
        => attachmentNames.Any(x => firearm.HasAttachment(x));

    public static void ToggleAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.ToggleAttachments((IEnumerable<AttachmentName>)attachmentNames);

    public static bool SetAttachments(this Firearm firearm, bool enabled, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, enabled);

    public static bool EnableAttachment(this Firearm firearm, AttachmentName attachmentName)
        => firearm.SetAttachment(attachmentName, true);

    public static bool EnableAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => firearm.SetAttachments(attachmentNames, true);

    public static bool EnableAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, true);

    public static bool DisableAttachment(this Firearm firearm, AttachmentName attachmentName)
        => firearm.SetAttachment(attachmentName, false);

    public static bool DisableAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => firearm.SetAttachments(attachmentNames, false);

    public static bool DisableAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, false);
    
    public static void SyncAttachments(this Firearm firearm)
        => firearm.ApplyAttachmentsCode(firearm.GetCurrentAttachmentsCode(), false);
    #endregion
    
    public static void PlayDisruptorAudio(this Firearm firearm, bool isSingleShot, bool wasLastCharge)
    {
        if (!firearm.TryGetModule<DisruptorAudioModule>(out var disruptorAudioModule))
            return;

        disruptorAudioModule.PlayDisruptorShot(isSingleShot, wasLastCharge);
    }
    
    public static void PlayPumpActionDryFireAudio(this Firearm firearm)
    {
        if (!firearm.TryGetModule<PumpActionModule>(out var pumpActionModule))
            return;

        pumpActionModule.PlaySound(x => x.PlayNormal(pumpActionModule._dryFireClip), false);
    }

    public static void PlayPumpActionGunShotAudio(this Firearm firearm, int? clipId = null)
    {
        if (!firearm.TryGetModule<PumpActionModule>(out var pumpActionModule))
            return;
        
        if (!clipId.HasValue || clipId.Value < 0 || clipId.Value >= pumpActionModule._shotClipPerBarrelIndex.Length)
            clipId = RandomGen.Instance.GetInt32(0, pumpActionModule._shotClipPerBarrelIndex.Length);

        pumpActionModule.PlaySound(x => x.PlayGunshot(pumpActionModule._shotClipPerBarrelIndex[clipId.Value]), false);
    }
    
    public static void PlayDoubleActionDryFireAudio(this Firearm firearm)
    {
        if (!firearm.TryGetModule<DoubleActionModule>(out var doubleActionModule))
            return;

        doubleActionModule._audioModule.PlayNormal(doubleActionModule._dryFireClip);
    }
    
    public static void PlayDoubleActionGunShotAudio(this Firearm firearm, int? clipId = null)
    {
        if (!firearm.TryGetModule<DoubleActionModule>(out var doubleActionModule))
            return;

        if (!clipId.HasValue || clipId.Value < 0 || clipId.Value >= doubleActionModule._fireClips.Length)
            clipId = RandomGen.Instance.GetInt32(0, doubleActionModule._fireClips.Length);

        doubleActionModule._audioModule.PlayGunshot(doubleActionModule._fireClips[clipId.Value]);
    }

    public static void PlayAutomaticDryFireAudio(this Firearm firearm)
    {
        if (!firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            return;

        automaticActionModule.PlayDryFire();
    }
    
    public static void PlayAutomaticGunShotAudio(this Firearm firearm, int shotChambersAmount)
    {
        if (!firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            return;

        automaticActionModule.PlayFire(shotChambersAmount);
    }
    
    public static void PlayDryFireAudio(this Firearm firearm)
    {
        if (firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            automaticActionModule.PlayDryFire();
        else if (firearm.TryGetModule<DoubleActionModule>(out var doubleActionModule))
            doubleActionModule._audioModule.PlayNormal(doubleActionModule._dryFireClip);
        else if (firearm.TryGetModule<PumpActionModule>(out var pumpActionModule))
            pumpActionModule.PlaySound(x => x.PlayNormal(pumpActionModule._dryFireClip), false);
    }

    public static void PlayGunShotAudio(this Firearm firearm)
    {
        if (firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            automaticActionModule.PlayFire(1);
        else if (firearm.TryGetModule<DisruptorAudioModule>(out var disruptorAudioModule))
            disruptorAudioModule.PlayDisruptorShot(true, false);
        else if (firearm.TryGetModule<DoubleActionModule>(out var doubleActionModule))
            doubleActionModule._audioModule.PlayGunshot(doubleActionModule._fireClips[(int)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride)]);
        else if (firearm.TryGetModule<PumpActionModule>(out var pumpActionModule))
            pumpActionModule.PlaySound(x =>
                x.PlayGunshot(pumpActionModule._shotClipPerBarrelIndex[
                    RandomGen.Instance.GetInt32(0, pumpActionModule._shotClipPerBarrelIndex.Length)]), false);
    }
    
    public static bool IsAiming(this Firearm firearm)
        => firearm.GetAdsModule().AdsTarget;

    public static bool IsAutomatic(this Firearm firearm)
        => firearm.HasModule<AutomaticActionModule>();

    public static bool IsFlashlightEnabled(this Firearm firearm)
        => firearm.IsEmittingLight;

    public static bool IsNightVisionEnabled(this Firearm firearm)
        => firearm.IsAiming() && firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.NightVision);

    public static float GetFireRate(this Firearm firearm)
        => firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule)
            ? automaticActionModule.BaseFireRate
            : 0f;

    public static bool SetFireRate(this Firearm firearm, float fireRate)
    {
        if (!firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            return false;

        automaticActionModule.BaseFireRate = fireRate;
        return true;
    }

    public static int GetAmmo(this Firearm firearm)
        => firearm.GetTotalStoredAmmo();

    public static void SetAmmo(this Firearm firearm, int ammo)
        => firearm.GetMagazineModule().AmmoStored = ammo;

    public static int GetMaxAmmo(this Firearm firearm)
        => firearm.GetTotalMaxAmmo();

    public static void SetMaxAmmo(this Firearm firearm, int ammo)
        => firearm.GetMagazineModule()._defaultCapacity = ammo;

    public static ItemType GetAmmoType(this Firearm firearm)
        => firearm.GetMagazineModule().AmmoType;
    
    public static bool ReloadAmmo(this Firearm firearm, bool emptyMagazine = false, bool playAnimationToOwner = false, bool playAnimationToEveryone = false)
    {
        if (!firearm.TryGetModule<MagazineModule>(out var magazineModule))
            return false;

        magazineModule.ServerRemoveMagazine();

        if ((playAnimationToOwner || playAnimationToEveryone) && firearm.TryGetModule<AnimatorReloaderModuleBase>(out var reloaderModuleBase))
        {
            reloaderModuleBase.SendRpc(x =>
            {
                x.WriteSubheader(AnimatorReloaderModuleBase.ReloaderMessageHeader.Reload);

                if (reloaderModuleBase._randomize)
                    x.WriteByte((byte)UnityEngine.Random.Range(byte.MinValue, byte.MaxValue + 1));
            }, playAnimationToEveryone);
        }

        Timing.CallDelayed(0.5f, () =>
        {
            if (emptyMagazine)
                magazineModule.ServerInsertEmptyMagazine();
            else
                magazineModule.ServerInsertMagazine();
        });

        return true;
    }

    public static float GetDamageAtDistance(this Firearm firearm, float distance)
    {
        if (firearm.TryGetModule<HitscanHitregModuleBase>(out var hitregModule))
            return hitregModule.DamageAtDistance(distance);

        return 0f;
    }

    public static float GetDamageFalloffDistance(this Firearm firearm)
    {
        if (firearm.TryGetModule<HitscanHitregModuleBase>(out var hitregModule))
            return hitregModule.DamageFalloffDistance;

        return 0f;
    }

    public static RecoilSettings GetRecoilSettings(this Firearm firearm)
        => firearm.TryGetModule<RecoilPatternModule>(out var recoilPatternModule)
            ? recoilPatternModule.BaseRecoil
            : default;

    public static BuckshotSettings GetBuckshotSettings(this Firearm firearm)
        => firearm.TryGetModule<BuckshotHitreg>(out var buckshotHitreg) 
            ? buckshotHitreg.BasePattern 
            : default;

    public static bool SetRecoilSettings(this Firearm firearm, RecoilSettings settings)
    {
        if (!firearm.TryGetModule<RecoilPatternModule>(out var recoilPatternModule))
            return false;

        recoilPatternModule.BaseRecoil = settings;
        return true;
    }

    public static bool SetBuckshotSettings(this Firearm firearm, BuckshotSettings settings)
    {
        if (!firearm.TryGetModule<BuckshotHitreg>(out var buckshotHitreg))
            return false;

        buckshotHitreg.BasePattern = settings;
        return true;
    }
    
    public static LinearAdsModule GetAdsModule(this Firearm firearm)
        => TryGetModule<LinearAdsModule>(firearm, out var adsModule) ? adsModule : null;
    
    public static MagazineModule GetMagazineModule(this Firearm firearm)
        => TryGetModule<MagazineModule>(firearm, out var module) ? module : null;

    public static bool HasModule<T>(this Firearm firearm)
        => firearm != null && TryGetModule<T>(firearm, out _);

    public static T GetModule<T>(this Firearm firearm)
        => TryGetModule<T>(firearm, out var module) ? module : default;

    public static Attachment GetAttachment(this Firearm firearm, AttachmentName attachmentName)
        => TryGetAttachment(firearm, attachmentName, out var attachment) ? attachment : null;

    public static bool TryGetAttachment(this Firearm firearm, AttachmentName attachmentName, out Attachment attachment)
    {
        attachment = null;

        for (int i = 0; i < firearm.Attachments.Length; i++)
        {
            var curAttachment = firearm.Attachments[i];

            if (curAttachment.Name != attachmentName)
                continue;

            attachment = curAttachment;
            return true;
        }

        return false;
    }

    public static bool TryGetModule<T>(this Firearm firearm, out T module)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        module = default;

        for (int i = 0; i < firearm.Modules.Length; i++)
        {
            var curModule = firearm.Modules[i];

            if (curModule is null || curModule is not T targetModule)
                continue;

            module = targetModule;
            return true;
        }

        return false;
    }
}