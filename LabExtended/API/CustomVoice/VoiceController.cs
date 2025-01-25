using LabExtended.Core.Hooking;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities.Unity;

using LabExtended.API.CustomVoice.Profiles;
using LabExtended.API.CustomVoice.Pitching;
using LabExtended.Core;
using LabExtended.Extensions;
using UnityEngine;

using VoiceChat;
using VoiceChat.Networking;

namespace LabExtended.API.CustomVoice;

public class VoiceController : IDisposable
{
    public static event Action<VoiceController> OnJoined; 
    
    private Dictionary<DateTime, VoiceMessage> _speakingPackets;
    private Dictionary<DateTime, VoiceMessage> _sessionPackets;
    
    private Dictionary<Type, VoiceProfile> _profiles;

    private bool _wasSpeaking;
    private float _speakingTime;
    
    public ExPlayer Player { get; }
    public VoicePitch Pitch { get; internal set; }

    public bool IsOnline => Player != null && Player;
    public bool IsSpeaking => IsOnline && Player.IsSpeaking;

    public IReadOnlyDictionary<DateTime, VoiceMessage> SpeakingPackets => _speakingPackets;
    public IReadOnlyDictionary<DateTime, VoiceMessage> SessionPackets => _sessionPackets;
    
    public IReadOnlyDictionary<Type, VoiceProfile> Profiles => _profiles;

    internal VoiceController(ExPlayer player)
    {
        Player = player;
        
        Pitch = new VoicePitch(this);

        _speakingPackets = DictionaryPool<DateTime, VoiceMessage>.Shared.Rent();
        _sessionPackets = DictionaryPool<DateTime, VoiceMessage>.Shared.Rent();
        _profiles = DictionaryPool<Type, VoiceProfile>.Shared.Rent();

        PlayerLoopHelper.AfterLoop += UpdateSpeaking;
        InternalEvents.OnSpawning += HandleSpawn;
        
        OnJoined.InvokeSafe(this);
    }

    public bool HasProfile<T>(out T profile) where T : VoiceProfile
    {
        if (Profiles.TryGetValue(typeof(T), out var instance))
        {
            profile = (T)instance;
            return true;
        }

        profile = null;
        return false;
    }
    
    public bool HasProfile<T>() where T : VoiceProfile
        => Profiles.ContainsKey(typeof(T));
    
    public bool HasProfile(Type profileType)
        => Profiles.ContainsKey(profileType);
    
    public bool HasProfile(Type profileType, out VoiceProfile profile)
        => Profiles.TryGetValue(profileType, out profile);
    
    public bool HasProfile(VoiceProfile profile)
        => Profiles.ContainsKey(profile.GetType());

    public T GetProfile<T>() where T : VoiceProfile
    {
        if (HasProfile<T>(out var profile))
            return profile;
        
        throw new Exception($"No profile found for {typeof(T).Name}");
    }

    public T GetOrAddProfile<T>(bool newEnableProfile = false) where T : VoiceProfile
    {
        if (HasProfile<T>(out var profile))
            return profile;

        return AddProfile<T>(newEnableProfile);
    }

    public T AddProfile<T>(bool enableProfile = false) where T : VoiceProfile
        => (T)AddProfile(typeof(T), enableProfile);

    public VoiceProfile GetOrAddProfile(Type profileType, bool newEnableProfile = false)
    {
        if (profileType is null)
            throw new ArgumentNullException(nameof(profileType));

        if (HasProfile(profileType, out var profile))
            return profile;
        
        return AddProfile(profileType, newEnableProfile);
    }

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
    
    public bool RemoveProfile<T>() where T : VoiceProfile
        => RemoveProfile(typeof(T));

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
    
    public void Dispose()
    {
        PlayerLoopHelper.AfterLoop -= UpdateSpeaking;
        InternalEvents.OnSpawning -= HandleSpawn;
        
        Pitch?.Dispose();
        Pitch = null;
        
        if (_speakingPackets != null)
        {
            DictionaryPool<DateTime, VoiceMessage>.Shared.Return(_speakingPackets);
            _speakingPackets = null;
        }
        
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
        if (!IsOnline)
            return;
        
        if (msg.SpeakerNull || msg.Speaker is null || msg.Speaker.netId != Player.NetId)
            return;

        var time = DateTime.Now;

        _speakingPackets.Add(time, msg);
        _sessionPackets.Add(time, msg);

        var origChannel = Player.Role.VoiceModule.ValidateSend(msg.Channel);

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

                var result = profile.Value.Receive(ref msg);

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

            if (!send || msg.Channel is VoiceChatChannel.None)
                continue;
            
            player.Send(msg);
        }
    }

    private void HandleSpawn(PlayerSpawningArgs args)
    {
        if (!Player || !args.Player || args.Player != Player)
            return;
        
        foreach (var profile in _profiles)
        {
            if (!profile.Value.OnChangingRole(args.NewRole))
            {        
                if (profile.Value.IsActive)
                {
                    profile.Value.IsActive = false;
                    profile.Value.Disable();
                }
            }
            else
            {
                if (!profile.Value.IsActive)
                {
                    profile.Value.Enable();
                    profile.Value.IsActive = true;
                }
            }
        }
    }

    private void UpdateSpeaking()
    {
        if (IsSpeaking == _wasSpeaking)
            return;
        
        if (_wasSpeaking)
        {
            HookRunner.RunEvent(new PlayerStoppedSpeakingArgs(Player, _speakingTime, _speakingPackets));
            VoiceEvents.InvokeOnStoppedSpeaking(Player, _speakingTime, _speakingPackets);
        }
        else
        {
            _speakingPackets.Clear();
            _speakingTime = Time.realtimeSinceStartup;

            HookRunner.RunEvent(new PlayerStartedSpeakingArgs(Player));
            VoiceEvents.InvokeOnStartedSpeaking(Player);
        }

        _wasSpeaking = !_wasSpeaking;
    }
    
    private VoiceChatChannel GetChannel(ExPlayer receiver, VoiceChatChannel messageChannel)
    {
        if (receiver.Role.VoiceModule is null || receiver == Player)
            return VoiceChatChannel.None;
        
        return receiver.Role.VoiceModule.ValidateReceive(Player.Hub, messageChannel);
    }
}