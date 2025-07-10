using LabExtended.API.Audio;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Commands.Custom.Audio;

[Command("audio", "Audio management commands.")]
public class AudioCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("create", "Creates a new audio handler.")]
    public void Create(
        [CommandParameter("Name", "Name of the audio handler.")] string name)
    {
        if (AudioManager.TryGetHandler(name, out _))
        {
            Fail($"Handler '{name}' already exists.");
            return;
        }

        var handler = AudioManager.GetOrAddHandler(name, x => x.AddPlayer());

        Ok($"Created handler '{handler.Name}'!");
    }

    [CommandOverload("destroy", "Destroys a specific audio handler.")]
    public void Destroy(
        [CommandParameter("Name", "Name of the audio handler.")] string name)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        handler.Destroy();

        Ok($"Destroyed handler '{name}'");
    }

    [CommandOverload("list", "Lists all audio handlers.")]
    public void List()
    {
        if (AudioManager.Handlers.Count == 0)
        {
            Ok("There aren't any audio handlers.");
            return;
        }

        Ok(x =>
        {
            x.AppendLine($"Showing '{AudioManager.Handlers.Count}' audio handler(s):");

            foreach (var pair in AudioManager.Handlers)
                x.AppendLine($"- {pair.Key}");
        });
    }

    [CommandOverload("add", "Add a speaker to a created audio handler.")]
    public void Add(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Speaker", "Name of the speaker to add.")] string speaker,
        [CommandParameter("ID", "Controller ID of the speaker.")] byte id)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        if (!handler.EnsureSpeaker(speaker, id))
        {
            Ok($"Speaker '{speaker}' exists in handler '{handler.Name}'");
            return;
        }

        Ok($"Added speaker '{speaker} ({id})'");
    }

    [CommandOverload("remove", "Removes a speaker from a created audio handler.")]
    public void Remove(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Speaker", "Name of the speaker to remove.")] string speaker)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        if (handler.DestroySpeaker(speaker))
        {
            Ok($"Speaker '{speaker}' removed from handler '{handler.Name}'");
            return;
        }

        Fail($"Could not remove speaker '{speaker}' from handler '{handler.Name}'");
    }

    [CommandOverload("speakers", "Lists all speakers in an audio handler.")]
    public void Speakers(
        [CommandParameter("Name", "Name of the audio handler.")] string name)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        if (handler.Speakers.Count == 0)
        {
            Fail($"Handler '{handler.Name}' does not have any speakers.");
            return;
        }

        Ok(x =>
        {
            x.AppendLine($"Audio Handler '{handler.Name}':");
            x.AppendLine($"- Controllers: {string.Join(", ", handler.Sources)}");
            x.AppendLine(
                $"- Speakers: {string.Join(", ", handler.Speakers.Select(y => $"{y.Key} ({y.Value.NetId})"))}");
        });
    }

    [CommandOverload("player", "Modifies a property on an audio player.")]
    public void Player(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Property", "Name of the audio player property to set.")] AudioPlayerProperty property,
        [CommandParameter("Value", "Value of the property")] string value = "")
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        if (handler.Player is null || handler.Player.IsDisposed)
        {
            Fail($"Handler '{handler.Name}' does not have an audio player (or is disposed).");
            return;
        }

        switch (property)
        {
            case AudioPlayerProperty.IsLooping:
                handler.Player.IsLooping = !handler.Player.IsLooping;

                Ok(handler.Player.IsLooping
                    ? $"Enabled looping on handler '{handler.Name}'"
                    : $"Disabled looping on handler '{handler.Name}'");
                return;

            case AudioPlayerProperty.IsPaused:
                handler.Player.IsPaused = !handler.Player.IsPaused;

                Ok(handler.Player.IsPaused ? $"Paused handler '{handler.Name}'" : $"Resumed handler '{handler.Name}'");
                return;

            case AudioPlayerProperty.Volume:
            {
                if (!float.TryParse(value, out var volume))
                {
                    Fail($"Could not parse volume value.");
                    return;
                }

                handler.Player.Volume = volume;

                Ok($"Set volume of handler '{handler.Name}' to '{handler.Player.Volume}'");
                return;
            }

            case AudioPlayerProperty.NextClip:
            {
                if (!AudioManager.TryGetClip(name, out var clip))
                {
                    Fail($"Clip '{value}' could not be found.");
                    return;
                }

                if (!handler.Player.IsPlaying)
                    handler.Player.Play(clip);
                else
                    handler.Player.NextClip = clip;

                Ok($"Set next clip of handler '{handler.Name}' to '{clip.Key}'");
                return;
            }
        }
    }

    [CommandOverload("speaker", "Modifies a property on a speaker.")]
    public void Speaker(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Speaker", "Name of the speaker to modify")] string speaker,
        [CommandParameter("Property", "Name of the audio speaker property to set.")] AudioSpeakerProperty property,
        [CommandParameter("Value", "Value of the property")] string value = "")
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }
        
        if (handler.Speakers.Count == 0)
        {
            Fail($"Handler '{handler.Name}' does not have any speakers.");
            return;
        }

        if (!handler.Speakers.TryGetValue(speaker, out var target))
        {
            Fail($"Speaker '{speaker}' could not be found on handler '{handler.Name}'");
            return;
        }

        switch (property)
        {
            case AudioSpeakerProperty.IsSpatial:
                target.IsSpatial = !target.IsSpatial;
                
                Ok(target.IsSpatial 
                    ? $"Enabled spatial audio for speaker '{speaker}' on handler '{handler.Name}'"
                    : $"Disabled spatial audio for speaker '{speaker}' on handler '{handler.Name}'");

                return;

            case AudioSpeakerProperty.ControllerId:
            {
                if (!byte.TryParse(value, out var controllerId))
                {
                    Fail("Could not parse new controller ID.");
                    return;
                }

                var curId = target.ControllerId;

                if (curId != controllerId)
                {
                    target.ControllerId = controllerId;

                    handler.Sources.AddUnique(controllerId);
                    handler.Sources.RemoveIfUnique(curId);
                }
                
                Ok($"Controller ID set to '{target.ControllerId}' on speaker '{speaker}' in handler '{handler.Name}'");
                return;
            }

            case AudioSpeakerProperty.MinDistance:
            {
                if (!float.TryParse(value, out var distance))
                {
                    Fail($"Could not parse new distance.");
                    return;
                }
                
                target.MinDistance = distance;
                
                Ok($"Set minimum distance of speaker '{speaker}' in handler '{handler.Name}' to '{distance}'");
                return;
            }
            
            case AudioSpeakerProperty.MaxDistance:
            {
                if (!float.TryParse(value, out var distance))
                {
                    Fail($"Could not parse new distance.");
                    return;
                }
                
                target.MaxDistance = distance;
                
                Ok($"Set maximum distance of speaker '{speaker}' in handler '{handler.Name}' to '{distance}'");
                return;
            }
            
            case AudioSpeakerProperty.Volume:
            {
                if (!float.TryParse(value, out var volume))
                {
                    Fail($"Could not parse new volume.");
                    return;
                }
                
                target.Volume = volume;
                
                Ok($"Set volue of speaker '{speaker}' in handler '{handler.Name}' to '{volume}'");
                return;
            }
        }
    }

    [CommandOverload("position", "Controls the position of a speaker.")]
    public void Position(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Filter", "List of speaker names to apply the position to.")] List<string> speakers,
        [CommandParameter("Position", "The new speaker position.")] Vector3 position)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }
        
        if (handler.Speakers.Count == 0)
        {
            Fail($"Handler '{handler.Name}' does not have any speakers.");
            return;
        }

        var count = 0;

        foreach (var pair in handler.Speakers)
        {
            if (speakers.Count == 0 || (speakers.Count == 1 && speakers[0] == "*") || speakers.Contains(pair.Key))
            {
                pair.Value.Position = position;

                count++;
            }
        }
        
        Ok($"Applied position to '{count}' speaker(s) in handler '{handler.Name}'");
    }

    [CommandOverload("play", "Plays an audio clip.")]
    public void Play(       
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Clip", "Name of the audio clip to play.")] string clip,
        [CommandParameter("Override", "Whether or not to override the currently playing clip (if any).")] bool overrideCurrent = true)
    {        
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }

        if (!AudioManager.TryGetClip(name, out var target))
        {
            Fail($"Clip '{clip}' could not be found.");
            return;
        }
        
        if (handler.Player is null || handler.Player.IsDisposed)
        {
            Fail($"Handler '{handler.Name}' does not have an audio player (or is disposed).");
            return;
        }
        
        var playing = handler.Player.Play(target, overrideCurrent);
        
        Ok((playing ? "Started playing "
                   : "Queued ") + $"audio clip '{target.Key}' on handler '{handler.Name}'");
    }

    [CommandOverload("stop", "Stops audio playback.")]
    public void Stop(
        [CommandParameter("Name", "Name of the audio handler.")] string name,
        [CommandParameter("Clear", "Whether or not to clear any audio clips in the queue.")] bool clearQueue = false)
    {
        if (!AudioManager.TryGetHandler(name, out var handler))
        {
            Fail($"Handler '{name}' does not exist.");
            return;
        }
        
        if (handler.Player is null || handler.Player.IsDisposed)
        {
            Fail($"Handler '{handler.Name}' does not have an audio player (or is disposed).");
            return;
        }

        if (!handler.Player.IsPlaying)
        {
            Fail($"Handler '{handler.Name}' is not playing any audio.");
            return;
        }
        
        handler.Player.Stop(clearQueue);
        
        Ok($"Stopped playback of audio handler '{handler.Name}'");
    }
}