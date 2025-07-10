using LabExtended.API.Toys;
using LabExtended.Extensions;

using Mirror;

using NorthwoodLib.Pools;

using UnityEngine;

using VoiceChat;
using VoiceChat.Networking;

namespace LabExtended.API.Audio;

/// <summary>
/// Helper class for audio playback.
/// </summary>
public class AudioHandler
{
    /// <summary>
    /// Gets the name of the handler.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the group's audio player.
    /// </summary>
    public AudioPlayer? Player { get; internal set; }

    /// <summary>
    /// Gets the list of speaker IDs to send the audio to.
    /// </summary>
    public List<byte> Sources { get; } = new();
    
    /// <summary>
    /// Gets the list of speakers spawned specifically for this group.
    /// </summary>
    public Dictionary<string, SpeakerToy> Speakers { get; } = new();

    /// <summary>
    /// Sends the specified encoded audio through each speaker defined in <see cref="Sources"/>.
    /// </summary>
    /// <param name="encodedAudio">The audio to send.</param>
    /// <param name="encodedLength">The length of the encoded audio.</param>
    /// <param name="receiveFilter">The predicate used to control which player receives the audio.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Transmit(byte[] encodedAudio, int encodedLength, Predicate<ExPlayer>? receiveFilter = null)
    {
        if (encodedAudio is null)
            throw new ArgumentNullException(nameof(encodedAudio));
        
        if (encodedAudio.Length < encodedLength)
            throw new ArgumentOutOfRangeException(nameof(encodedLength));
        
        if (encodedLength > VoiceChatSettings.PacketSizePerChannel)
            throw new ArgumentOutOfRangeException(nameof(encodedLength));

        if (Sources.Count < 1)
            return;

        var message = new AudioMessage(0, encodedAudio, encodedLength);

        for (var i = 0; i < Sources.Count; i++)
        {
            message.ControllerId = Sources[i];

            for (var x = 0; x < ExPlayer.Players.Count; x++)
            {
                var player = ExPlayer.Players[x];
                
                if (player?.ReferenceHub == null || player.IsUnverified)
                    continue;
                
                if (receiveFilter != null && !receiveFilter(player))
                    continue;
                
                player.Connection.Send(message);
            }
        }
    }
    
    /// <summary>
    /// Plays the specific audio clip on the specified transform.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="target">The parent transform to play the clip at.</param>
    /// <param name="localPosition">The position of the speaker relevant to the target parent.</param>
    /// <param name="localRotation">The rotation of the speaker relevant to the target parent.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void PlayOn(KeyValuePair<string, byte[]> clip, NetworkIdentity target, Vector3? localPosition = null, Quaternion? localRotation = null)
    {
        if (clip.Value is null)
            throw new ArgumentNullException(nameof(clip));

        if (target?.gameObject == null)
            throw new ArgumentNullException(nameof(target));
        
        if (Player is null)
            throw new Exception("Attempted to play on a disposed or incorrectly setup audio handler.");

        foreach (var speaker in Speakers)
        {
            speaker.Value.Parent = target;

            if (localPosition.HasValue)
                speaker.Value.Position = localPosition.Value;
            
            if (localRotation.HasValue)
                speaker.Value.Rotation = localRotation.Value;
        }

        Player.Play(clip, true);
    }
    
    /// <summary>
    /// Plays the specific audio clip at the specified position.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="position">The position to play the clip at.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void PlayAt(KeyValuePair<string, byte[]> clip, Vector3 position)
    {
        if (clip.Value is null)
            throw new ArgumentNullException(nameof(clip));

        if (Player is null)
            throw new Exception("Attempted to play on a disposed or incorrectly setup audio handler.");

        foreach (var speaker in Speakers)
            speaker.Value.Position = position;
        
        Player.Play(clip, true);
    }

    /// <summary>
    /// Adds a new audio player to the group.
    /// </summary>
    /// <returns>true if the audio player was added</returns>
    public bool AddPlayer()
    {
        if (Player != null && !Player.IsDisposed)
            return false;

        Player = new();
        
        AudioManager.Update += Update;
        return true;
    }

    /// <summary>
    /// Whether or not the group has a specific speaker ID.
    /// </summary>
    /// <param name="speakerId">The speaker ID to check.</param>
    /// <returns>true if the speaker ID was found in <see cref="Sources"/></returns>
    public bool HasSpeaker(byte speakerId)
        => Sources.Contains(speakerId);

    /// <summary>
    /// Whether or not the group has a specific speaker's ID.
    /// </summary>
    /// <param name="toy">The speaker to check.</param>
    /// <returns>true if the speaker's ID was found in <see cref="Sources"/></returns>
    public bool HasSpeaker(SpeakerToy toy)
        => toy?.Base != null && Sources.Contains(toy.ControllerId);

    /// <summary>
    /// Whether or not the group has spawned a speaker with a specific custom ID.
    /// </summary>
    /// <param name="name">The custom ID of the speaker.</param>
    /// <returns>true if the custom ID was found in <see cref="Speakers"/></returns>
    public bool HasSpeaker(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        
        return Speakers.ContainsKey(name);
    }
    
    /// <summary>
    /// Whether or not the group has spawned a speaker with a specific custom ID.
    /// </summary>
    /// <param name="name">The custom ID of the speaker.</param>
    /// <param name="toy">The found speaker instance.</param>
    /// <returns>true if the custom ID was found in <see cref="Speakers"/></returns>
    public bool HasSpeaker(string name, out SpeakerToy toy)
    {
        toy = null;
        
        if (string.IsNullOrEmpty(name))
            return false;
        
        return Speakers.TryGetValue(name, out toy);
    }

    /// <summary>
    /// Adds an already spawned speaker to the list (or replaces an existing one with the same name).
    /// </summary>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="speakerToy">The speaker toy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddExistingSpeaker(string speakerName, SpeakerToy speakerToy)
    {
        if (string.IsNullOrEmpty(speakerName))
            throw new ArgumentNullException(nameof(speakerName));
        
        if (speakerToy?.Base == null)
            throw new ArgumentNullException(nameof(speakerToy));

        Sources.AddUnique(speakerToy.ControllerId);

        Speakers[speakerName] = speakerToy;
    }
    
    /// <summary>
    /// Ensures that a speaker with a specific name exists (and configures it).
    /// </summary>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="speakerId">The ID of the speaker.</param>
    /// <param name="position">The speaker position.</param>
    /// <param name="isSpatial">Whether or not the speaker's audio should be spatial (3D).</param>
    /// <param name="volume">The speaker's volume (range from 0 to 1).</param>
    /// <param name="minDistance">The minimum distance you need to have from the speaker to hear it.</param>
    /// <param name="maxDistance">The maximum distance you can have from the speaker to hear it.</param>
    /// <returns>true if a new speaker was spawned</returns>
    public bool ConfigureSpeaker(string speakerName, byte speakerId, Vector3 position, bool isSpatial, float volume, float minDistance, float maxDistance)
    {
        return ConfigureSpeaker(speakerName, speakerId, (speaker, _) =>
        {
            speaker.Position = position;
            
            speaker.MinDistance = minDistance;
            speaker.MaxDistance = maxDistance;
            
            speaker.IsSpatial = isSpatial;

            speaker.Volume = volume;
        });
    }
    
    /// <summary>
    /// Ensures that a speaker with a specific name exists (and configures it).
    /// </summary>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="speakerId">The ID of the speaker.</param>
    /// <param name="configuration">The delegate invoked to configure the speaker. The second parameter defines if the speaker is new (did not exist - true).</param>
    /// <returns>true if a new speaker was spawned</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool ConfigureSpeaker(string speakerName, byte speakerId, Action<SpeakerToy, bool> configuration)
    {
        if (string.IsNullOrEmpty(speakerName))
            throw new ArgumentNullException(nameof(speakerName));

        if (!Speakers.TryGetValue(speakerName, out var speaker))
        {
            speaker = new SpeakerToy();
            speaker.ControllerId = speakerId;

            Sources.Add(speakerId);
            Speakers.Add(speakerName, speaker);
            
            configuration?.InvokeSafe(speaker, true);
        }
        else
        {
            configuration?.InvokeSafe(speaker, false);
        }

        return true;
    }

    /// <summary>
    /// Ensures that a speaker with a specific name exists.
    /// </summary>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="speakerId">The ID of the speaker.</param>
    /// <param name="position">The speaker position.</param>
    /// <param name="isSpatial">Whether or not the speaker's audio should be spatial (3D).</param>
    /// <param name="volume">The speaker's volume (range from 0 to 1).</param>
    /// <param name="minDistance">The minimum distance you need to have from the speaker to hear it.</param>
    /// <param name="maxDistance">The maximum distance you can have from the speaker to hear it.</param>
    /// <returns>true if a new speaker was spawned</returns>
    public bool EnsureSpeaker(string speakerName, byte speakerId, Vector3 position, bool isSpatial, float volume, float minDistance, float maxDistance)
    {
        return EnsureSpeaker(speakerName, speakerId, speaker =>
        {
            speaker.Position = position;
            
            speaker.MinDistance = minDistance;
            speaker.MaxDistance = maxDistance;
            
            speaker.IsSpatial = isSpatial;

            speaker.Volume = volume;
        });
    }

    /// <summary>
    /// Ensures that a speaker with a specific name exists.
    /// </summary>
    /// <param name="speakerName">The name of the speaker.</param>
    /// <param name="speakerId">The ID of the speaker.</param>
    /// <param name="configuration">The delegate invoked to configure the speaker.</param>
    /// <returns>true if a new speaker was spawned</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool EnsureSpeaker(string speakerName, byte speakerId, Action<SpeakerToy>? configuration = null)
    {
        if (string.IsNullOrEmpty(speakerName))
            throw new ArgumentNullException(nameof(speakerName));

        if (Speakers.ContainsKey(speakerName))
            return false;

        var speaker = new SpeakerToy();

        speaker.ControllerId = speakerId;
        
        configuration?.InvokeSafe(speaker);
        
        Sources.Add(speakerId);
        Speakers.Add(speakerName, speaker);
        
        return true;
    }

    /// <summary>
    /// Destroys a speaker with a specific custom ID.
    /// </summary>
    /// <param name="speakerName">The ID of the speaker.</param>
    /// <returns>true if the speaker was destroyed</returns>
    public bool DestroySpeaker(string speakerName)
    {
        if (Speakers.TryGetValue(speakerName, out var speaker))
        {
            if (Speakers.Count(x => x.Value.ControllerId == speaker.ControllerId) < 2)
                Sources.Remove(speaker.ControllerId);
            
            NetworkServer.Destroy(speaker.GameObject);

            Speakers.Remove(speakerName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Destroys all speakers matching an ID.
    /// </summary>
    /// <param name="speakerId">The ID.</param>
    /// <returns>The amount of destroyed speakers.</returns>
    public int DestroySpeakers(byte speakerId)
    {
        if (Sources.Contains(speakerId))
        {
            Sources.Remove(speakerId);
            
            var list = ListPool<string>.Shared.Rent();
            var count = 0;
            
            foreach (var speaker in Speakers)
            {
                if (speaker.Value.ControllerId == speakerId)
                {
                    list.Add(speaker.Key);
                    
                    NetworkServer.Destroy(speaker.Value.GameObject);
                    
                    count++;
                }
            }
            
            list.ForEach(key => Speakers.Remove(key));
            
            ListPool<string>.Shared.Return(list);
            return count;
        }

        return 0;
    }

    /// <summary>
    /// Destroys all speakers attached to this group.
    /// </summary>
    /// <returns>The amount of destroyed speakers.</returns>
    public int DestroySpeakers()
    {
        var count = Speakers.Count;
        
        Sources.Clear();

        if (count > 0)
        {
            foreach (var pair in Speakers)
            {
                NetworkServer.Destroy(pair.Value.GameObject);
            }
            
            Speakers.Clear();
        }

        return count;
    }

    /// <summary>
    /// Destroys the audio handler.
    /// </summary>
    public void Destroy()
    {
        if (Name != null)
        {
            if (Player != null)
            {
                AudioManager.Update -= Update;

                Player.Dispose();
                Player = null;
            }

            DestroySpeakers();

            Sources.Clear();
            Speakers.Clear();

            AudioManager.Handlers.Remove(Name);

            Name = null;
        }
    }

    internal void InternalRemovePlayer()
    {
        Player = null;
        
        AudioManager.Update -= Update;
    }

    private void Update()
    {
        Player?.Update();
    }
}