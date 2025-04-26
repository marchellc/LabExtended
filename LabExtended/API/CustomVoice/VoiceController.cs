using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities.Unity;

using LabExtended.API.CustomVoice.Profiles;
using LabExtended.API.CustomVoice.Threading;

using LabExtended.Extensions;

using UnityEngine;

using VoiceChat;
using VoiceChat.Networking;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.API.CustomVoice;

/// <summary>
/// Used to control voice chat.
/// </summary>
public class VoiceController : IDisposable
{
    /// <summary>
    /// Gets called when a player joins and their VoiceController is ready.
    /// </summary>
    public static event Action<VoiceController>? OnJoined; 
    
    private Dictionary<DateTime, VoiceMessage> _sessionPackets;
    private Dictionary<Type, VoiceProfile> _profiles;

    private bool _wasSpeaking;
    private float _speakingTime;
    
    /// <summary>
    /// Gets the player that owns this controller.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the voice chat processing thread.
    /// </summary>
    public VoiceThread Thread { get; internal set; }

    /// <summary>
    /// Whether or not the player is online.
    /// </summary>
    public bool IsOnline => Player != null && Player;
    
    /// <summary>
    /// Whether or not the player is speaking.
    /// </summary>
    public bool IsSpeaking => IsOnline && Player.IsSpeaking;
    
    /// <summary>
    /// Gets all packets sent in a time window.
    /// </summary>
    public IReadOnlyDictionary<DateTime, VoiceMessage> SessionPackets => _sessionPackets;
    
    /// <summary>
    /// Gets all registered profiles.
    /// </summary>
    public IReadOnlyDictionary<Type, VoiceProfile> Profiles => _profiles;

    internal VoiceController(ExPlayer player)
    {
        Player = player;
        
        Thread = new VoiceThread(this);
        
        _sessionPackets = DictionaryPool<DateTime, VoiceMessage>.Shared.Rent();
        _profiles = DictionaryPool<Type, VoiceProfile>.Shared.Rent();

        PlayerLoopHelper.AfterLoop += UpdateSpeaking;
        InternalEvents.OnSpawning += HandleSpawn;
        
        OnJoined.InvokeSafe(this);
    }

    /// <summary>
    /// Tries to get a voice profile instance of a specific type.
    /// </summary>
    /// <param name="profile">The found profile instance.</param>
    /// <typeparam name="T">The type of profile to find.</typeparam>
    /// <returns>true if the profile instance was found</returns>
    public bool HasProfile<T>(out T? profile) where T : VoiceProfile
    {
        if (Profiles.TryGetValue(typeof(T), out var instance))
        {
            profile = (T)instance;
            return true;
        }

        profile = null;
        return false;
    }
    
    /// <summary>
    /// Whether or not this controller has a specific voice profile type.
    /// </summary>
    /// <typeparam name="T">The type of voice profile.</typeparam>
    /// <returns>true if the voice profile instance was found</returns>
    public bool HasProfile<T>() where T : VoiceProfile
        => Profiles.ContainsKey(typeof(T));
    
    /// <summary>
    /// Whether or not this controller has a specific voice profile type.
    /// </summary>
    /// <param name="profileType">The type of voice profile.</param>
    /// <returns>true if the voice profile instance was found</returns>
    public bool HasProfile(Type profileType)
        => Profiles.ContainsKey(profileType);
    
    /// <summary>
    /// Whether or not this controller has a specific voice profile type.
    /// </summary>
    /// <param name="profileType">The type of voice profile.</param>
    /// <param name="profile">Found voice profile instance.</param>
    /// <returns>true if the voice profile instance was found</returns>
    public bool HasProfile(Type profileType, out VoiceProfile profile)
        => Profiles.TryGetValue(profileType, out profile);
    
    /// <summary>
    /// Whether or not this controller has a specific voice profile type.
    /// </summary>
    /// <param name="profile">The profile instance to check.</param>
    /// <returns>true if the controller has a profile of the same type</returns>
    public bool HasProfile(VoiceProfile profile)
        => Profiles.ContainsKey(profile.GetType());

    /// <summary>
    /// Gets an active voice profile.
    /// </summary>
    /// <typeparam name="T">The type of voice profile.</typeparam>
    /// <returns>Found voice profile instance.</returns>
    /// <exception cref="Exception"></exception>
    public T GetProfile<T>() where T : VoiceProfile
    {
        if (HasProfile<T>(out var profile))
            return profile;
        
        throw new Exception($"No profile found for {typeof(T).Name}");
    }

    /// <summary>
    /// Gets (or adds) a voice profile of a specific type.
    /// </summary>
    /// <param name="newEnableProfile">Whether or not to automatically enable the profile if it's newly made.</param>
    /// <typeparam name="T">Type of the profile.</typeparam>
    /// <returns>The created / found profile instance.</returns>
    public T GetOrAddProfile<T>(bool newEnableProfile = false) where T : VoiceProfile
        => HasProfile<T>(out var profile) ? profile : AddProfile<T>(newEnableProfile);

    /// <summary>
    /// Adds a new voice profile of a specific type.
    /// </summary>
    /// <param name="enableProfile">Whether or not to automatically enable the profile.</param>
    /// <typeparam name="T">Type of the profile.</typeparam>
    /// <returns>The created profile instance.</returns>
    public T AddProfile<T>(bool enableProfile = false) where T : VoiceProfile
        => (T)AddProfile(typeof(T), enableProfile);

    /// <summary>
    /// Gets (or adds) a voice profile of a specific type.
    /// </summary>
    /// <param name="profileType">The voice profile type.</param>
    /// <param name="newEnableProfile">Whether or not to automatically enable the profile if a new one is made.</param>
    /// <returns>The created / found voice profile.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public VoiceProfile GetOrAddProfile(Type profileType, bool newEnableProfile = false)
    {
        if (profileType is null)
            throw new ArgumentNullException(nameof(profileType));

        return HasProfile(profileType, out var profile) ? profile : AddProfile(profileType, newEnableProfile);
    }

    /// <summary>
    /// Adds a new profile of a specific type.
    /// </summary>
    /// <param name="profileType">The type of the profile.</param>
    /// <param name="enableProfile">Whether or not to automatically enable the profile.</param>
    /// <returns>The created profile instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public VoiceProfile AddProfile(Type profileType, bool enableProfile = false)
    {
        if (profileType is null)
            throw new ArgumentNullException(nameof(profileType));
        
        if (Profiles.ContainsKey(profileType))
            throw new Exception($"Profile {profileType.Name} already added");
        
        var profile = Activator.CreateInstance(profileType) as VoiceProfile;

        if (profile is null)
            throw new Exception($"Type {profileType.FullName} could not be cast to VoiceProfile");

        profile.Player = Player;
        profile.Start();

        if (enableProfile)
        {
            profile.Enable();
            profile.IsActive = true;
        }

        _profiles.Add(profileType, profile);
        return profile;
    }
    
    /// <summary>
    /// Removes a profile of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of voice profile to remove.</typeparam>
    /// <returns>true if the profile was removed</returns>
    public bool RemoveProfile<T>() where T : VoiceProfile
        => RemoveProfile(typeof(T));

    /// <summary>
    /// Removes a profile of a specific type.
    /// </summary>
    /// <param name="profileType">The type to remove.</param>
    /// <returns>true if the profile was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveProfile(Type profileType)
    {
        if (profileType is null)
            throw new ArgumentNullException(nameof(profileType));

        if (!Profiles.TryGetValue(profileType, out var profile))
            return false;
        
        if (profile.IsActive)
            profile.Disable();
        
        profile.Stop();
        return _profiles.Remove(profileType);
    }

    /// <summary>
    /// Removes all voice profiles.
    /// </summary>
    public void RemoveProfiles()
    {
        foreach (var profile in _profiles)
        {
            if (profile.Value.IsActive)
                profile.Value.Disable();

            profile.Value.Stop();
        }
        
        _profiles.Clear();
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerLoopHelper.AfterLoop -= UpdateSpeaking;
        InternalEvents.OnSpawning -= HandleSpawn;
        
        Thread?.Dispose();
        Thread = null;
        
        if (_sessionPackets != null)
        {
            DictionaryPool<DateTime, VoiceMessage>.Shared.Return(_sessionPackets);
            _sessionPackets = null;
        }

        if (_profiles != null)
        {        
            foreach (var profile in _profiles)
            {
                if (profile.Value.IsActive)
                    profile.Value.Disable();

                profile.Value.Stop();
            }
            
            DictionaryPool<Type, VoiceProfile>.Shared.Return(_profiles);
            
            _profiles = null;
        }
    }

    internal void ProcessMessage(ref VoiceMessage msg)
    {
        if (!IsOnline) return;
        if (msg.Speaker is null || msg.Speaker.netId != Player.NetworkId) return;

        _sessionPackets.Add(DateTime.Now, msg);

        var origChannel = Player.Role.VoiceModule.ValidateSend(msg.Channel);
        
        ExVoiceChatEvents.OnSendingVoiceMessage(Player, ref msg);

        foreach (var profile in _profiles)
        {
            if (!profile.Value.IsActive)
                continue;

            var result = profile.Value.ReceiveFrom(ref msg);

            if (result is VoiceProfileResult.SkipAndDontSend)
                return;

            if (result is VoiceProfileResult.SkipAndSend)
                break;
        }

        for (int i = 0; i < ExPlayer.Players.Count; i++)
        {
            var player = ExPlayer.Players[i];
            var send = true;
            
            if (!player)
                continue;
            
            msg.Channel = GetChannel(player, origChannel);

            foreach (var profile in _profiles)
            {
                if (!profile.Value.IsActive)
                    continue;

                var result = profile.Value.SendTo(ref msg, player);

                if (result is VoiceProfileResult.None)
                    continue;

                if (result is VoiceProfileResult.SkipAndSend)
                    break;

                if (result is VoiceProfileResult.SkipAndDontSend)
                {
                    send = false;
                    break;
                }
            }

            if (send)
                ExVoiceChatEvents.OnReceivingVoiceMessage(Player, player, ref msg);
            
            if (!send || msg.Channel is VoiceChatChannel.None)
                continue;

            var receivingArgs = new PlayerReceivingVoiceMessageEventArgs(player.ReferenceHub, ref msg);
            
            PlayerEvents.OnReceivingVoiceMessage(receivingArgs);

            if (!receivingArgs.IsAllowed)
                continue;
            
            player.Send(msg);
        }
    }

    private void HandleSpawn(PlayerSpawningEventArgs args)
    {
        if (args.Player is not ExPlayer player)
            return;

        if (player != Player)
            return;
        
        foreach (var profile in _profiles)
        {
            if (!profile.Value.OnChangingRole(args.Role.RoleTypeId))
            {
                if (!profile.Value.IsActive) 
                    continue;
                
                profile.Value.IsActive = false;
                profile.Value.Disable();
            }
            else
            {
                if (profile.Value.IsActive)
                    continue;
                
                profile.Value.Enable();
                profile.Value.IsActive = true;
            }
        }
    }

    private void UpdateSpeaking()
    {
        if (IsSpeaking == _wasSpeaking)
            return;
        
        if (_wasSpeaking)
        {
            ExVoiceChatEvents.OnStoppedSpeaking(Player, _speakingTime, _sessionPackets);
        }
        else
        {
            _sessionPackets.Clear();
            _speakingTime = Time.realtimeSinceStartup;

            ExVoiceChatEvents.OnStartedSpeaking(Player);
        }

        _wasSpeaking = !_wasSpeaking;
    }
    
    private VoiceChatChannel GetChannel(ExPlayer receiver, VoiceChatChannel messageChannel)
    {
        if (receiver.Role.VoiceModule is null)
            return VoiceChatChannel.None;

        if (receiver == Player && messageChannel != VoiceChatChannel.Mimicry)
            return receiver.Toggles is { CanHearSelf: true, CanBeHeard: true } 
                ? VoiceChatChannel.RoundSummary 
                : VoiceChatChannel.None;

        if (Player.Toggles.CanBeHeard)
            return receiver.Role.VoiceModule.ValidateReceive(Player.ReferenceHub, messageChannel);
        
        if ((Player.Toggles.CanBeHeardBySpectators && receiver.IsSpectating(Player))
            || (Player.Toggles.CanBeHeardByStaff && (receiver.IsNorthwoodStaff || receiver.HasRemoteAdminAccess))
            || (Player.Toggles.CanBeHeardByOtherRoles && receiver.Role.Type != Player.Role.Type))
            return receiver.Role.VoiceModule.ValidateReceive(Player.ReferenceHub, messageChannel);
            
        return VoiceChatChannel.None;
    }
}