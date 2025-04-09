using AudioPooling;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules.Misc;

using LabExtended.API;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;

using UnityEngine;

// ReSharper disable CompareOfFloatsByEqualityOperator

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions targeting firearm audio playback.
/// </summary>
public static class FirearmAudioExtensions
{
    /// <summary>
    /// A modified method of playing firearm audio clips (<see cref="AudioModule.ProcessEvent"/>) that allows the owner to optionally receive the sound.
    /// </summary>
    /// <param name="audioModule">The target audio module.</param>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="mixerChannel">The audio mixer channel.</param>
    /// <param name="audioRange">The maximum hearable audio range.</param>
    /// <param name="applyPitch">Whether or not to apply pitch to the audio.</param>
    /// <param name="sendToOwner">Whether or not to send the audio to the firearm's owner.</param>
    public static void PlayAudioClip(this AudioModule audioModule, AudioClip clip, MixerChannel mixerChannel = MixerChannel.Weapons,
        float audioRange = 40f, bool applyPitch = false, bool sendToOwner = false)
    {
        if (audioModule is null)
            return;

        if (clip is null)
            throw new ArgumentNullException(nameof(clip));
        
        var pitch = (applyPitch && FirearmEvent.CurrentlyInvokedEvent != null)
            ? FirearmEvent.CurrentlyInvokedEvent.LastInvocation.ParamSpeed
            : 1f;
        
        if (audioModule._clipToIndex.TryGetValue(clip, out var clipIndex))
            audioModule.SendAudioRpc(clipIndex, mixerChannel, audioRange, pitch, sendToOwner);
    }
    
    /// <summary>
    /// A modified method of sending audio RPCs (<see cref="AudioModule.ServerSendToNearbyPlayers"/>) that allows the owner to optionally receive it.
    /// </summary>
    /// <param name="audioModule">The target audio module.</param>
    /// <param name="index">Audio clip index.</param>
    /// <param name="channel">Audio mixer channel.</param>
    /// <param name="audioRange">Maximum hearable audio range.</param>
    /// <param name="pitch">Audio pitch.</param>
    /// <param name="sendToOwner">Whether or not to send the RPC to the owner.</param>
    public static void SendAudioRpc(this AudioModule audioModule, int index, MixerChannel channel, float audioRange, 
        float pitch, bool sendToOwner = false)
    {
        if (audioModule?.Firearm?.Owner?.roleManager.CurrentRole is not IFpcRole fpcRole)
            return;

        var distance = audioRange + 20f;
        var distanceSquared = distance * distance;
            
        var ownerPosition = fpcRole.FpcModule.Position;
        
        ExPlayer.Players.ForEach(ply =>
        {
            if (!sendToOwner && ply.ReferenceHub == audioModule.Firearm.Owner)
                return;

            if (!ply.Role.Is<IFpcRole>(out var targetRole))
                return;

            if (targetRole.SqrDistanceTo(ownerPosition) > distanceSquared)
                return;
            
            audioModule.SendRpc(x =>
            {
                var isVisible = targetRole is not ICustomVisibilityRole customVisibilityRole ||
                                customVisibilityRole.VisibilityController.ValidateVisibility(audioModule.Firearm.Owner);
                
                audioModule.ServerSend(x, index, pitch, channel, audioRange, ownerPosition, isVisible);
            });
        });
    }

    /// <summary>
    /// Plays the audio clip of the firearm dry-firing.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="sendToOwner">Whether or not to send the audio to the firearm's owner.</param>
    /// <returns>true if the audio was successfully played</returns>
    public static bool PlayDryFireAudioClip(this Firearm firearm, bool sendToOwner = false)
    {       
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));
        
        if (!firearm.TryGetModule<AudioModule>(out var audioModule))
            return false;

        for (var i = 0; i < firearm._modules.Length; i++)
        {
            var module = firearm._modules[i];

            switch (module)
            {
                case AutomaticActionModule automaticActionModule:
                    audioModule.PlayAudioClip(automaticActionModule._dryfireSound, MixerChannel.DefaultSfx, 12f, 
                        true, sendToOwner);
                    return true;
                
                case DoubleActionModule doubleActionModule:
                    audioModule.PlayAudioClip(doubleActionModule._dryFireClip, MixerChannel.DefaultSfx, 12f, 
                        true, sendToOwner);
                    return true;
                
                case PumpActionModule pumpActionModule:
                    audioModule.PlayAudioClip(pumpActionModule._dryFireClip, MixerChannel.DefaultSfx, 12f, true, 
                        sendToOwner);
                    return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Plays the audio clip of a gun shot.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="sendToOwner">Whether or not to send the audio to the firearm's owner.</param>
    /// <returns>true if the audio was successfully played</returns>
    public static bool PlayGunShotAudioClip(this Firearm firearm, bool sendToOwner = false)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));
        
        for (var i = 0; i < firearm._modules.Length; i++)
        {
            var module = firearm._modules[i];

            if (module is AutomaticActionModule automaticActionModule)
            {
                if (!firearm.TryGetModule<AudioModule>(out var audioModule))
                    return false;
                
                var clipId = firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride);
                var clips = automaticActionModule._gunshotSounds;

                for (var x = 0; x < clips.Length; x++)
                {
                    var clip = clips[x];
                    
                    if (clip.ClipId != clipId)
                        continue;
                    
                    audioModule.PlayAudioClip(clip.RandomSounds.RandomItem(), MixerChannel.Weapons, audioModule.FinalGunshotRange, 
                        false, sendToOwner);
                    return true;
                }
            }
            else if (module is DisruptorAudioModule disruptorAudioModule)
            {
                disruptorAudioModule.PlayAudioClip(disruptorAudioModule._singleShotAudio.ClipFiringNormal, 
                    MixerChannel.Weapons, disruptorAudioModule.FinalGunshotRange, false, sendToOwner);
                disruptorAudioModule.PlayAudioClip(disruptorAudioModule._singleShotAudio.ClipActionNormal, 
                    MixerChannel.DefaultSfx, 12f, true, sendToOwner);

                return true;
            }
            else if (module is DoubleActionModule doubleActionModule)
            {
                if (!firearm.TryGetModule<AudioModule>(out var audioModule))
                    return false;
                
                audioModule.PlayAudioClip(
                    doubleActionModule._fireClips[(int)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride)],
                    MixerChannel.Weapons, audioModule.FinalGunshotRange, false, sendToOwner);
                return true;
            }
            else if (module is PumpActionModule pumpActionModule)
            {
                if (!firearm.TryGetModule<AudioModule>(out var audioModule))
                    return false;
                
                audioModule.PlayAudioClip(pumpActionModule._shotClipPerBarrelIndex.RandomItem(), MixerChannel.Weapons, 
                    audioModule.FinalGunshotRange, false, sendToOwner);
                return true;
            }
        }

        return false;
    }
}