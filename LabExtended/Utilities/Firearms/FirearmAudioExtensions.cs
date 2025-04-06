using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Attachments;

using LabExtended.Utilities.Generation;

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions targeting firearm audio playback.
/// </summary>
public static class FirearmAudioExtensions
{
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
}