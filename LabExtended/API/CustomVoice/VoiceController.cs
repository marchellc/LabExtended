using LabExtended.Core.Hooking;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities.Unity;

using LabExtended.API.CustomVoice.Profiles;
using LabExtended.API.CustomVoice.Pitching;

using UnityEngine;

using VoiceChat;
using VoiceChat.Networking;

namespace LabExtended.API.CustomVoice;

public class VoiceController : IDisposable
{
    private Dictionary<float, VoiceMessage> _speakingPackets;
    private Dictionary<float, VoiceMessage> _sessionPackets;
    private Dictionary<Type, VoiceProfile> _profiles;

    private bool _wasSpeaking;
    private float _speakingTime;
    
    public ExPlayer Player { get; }
    public VoicePitch Pitch { get; internal set; }

    public bool IsOnline => Player != null && Player;
    public bool IsSpeaking => IsOnline && Player.IsSpeaking;

    public VoiceFlags Flags { get; set; } = VoiceFlags.None;

    public IReadOnlyDictionary<float, VoiceMessage> SpeakingPackets => _speakingPackets;
    public IReadOnlyDictionary<float, VoiceMessage> SessionPackets => _sessionPackets;
    
    public IReadOnlyDictionary<Type, VoiceProfile> Profiles => _profiles;

    public VoiceController(ExPlayer player)
    {
        Player = player;
        
        Pitch = new VoicePitch(this);

        _speakingPackets = DictionaryPool<float, VoiceMessage>.Shared.Rent();
        _sessionPackets = DictionaryPool<float, VoiceMessage>.Shared.Rent();
        _profiles = DictionaryPool<Type, VoiceProfile>.Shared.Rent();

        PlayerLoopHelper.AfterLoop += UpdateSpeaking;
        InternalEvents.OnSpawning += HandleSpawn;
    }
    
    public void Dispose()
    {
        PlayerLoopHelper.AfterLoop -= UpdateSpeaking;
        InternalEvents.OnSpawning -= HandleSpawn;
        
        Pitch?.Dispose();
        Pitch = null;
        
        if (_speakingPackets != null)
        {
            DictionaryPool<float, VoiceMessage>.Shared.Return(_speakingPackets);
            _speakingPackets = null;
        }
        
        if (_sessionPackets != null)
        {
            DictionaryPool<float, VoiceMessage>.Shared.Return(_sessionPackets);
            _sessionPackets = null;
        }

        if (_profiles != null)
        {
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

        var time = Time.realtimeSinceStartup;

        _speakingPackets[time] = msg;
        _sessionPackets[time] = msg;

        var origChannel = Player.Role.VoiceModule.ValidateSend(msg.Channel);

        for (int i = 0; i < ExPlayer._players.Count; i++)
        {
            var player = ExPlayer._players[i];
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

            if (!send)
                continue;
            
            player.Connection.Send(msg);
        }
    }

    private void HandleSpawn(PlayerSpawningArgs args)
    {
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
        if (receiver.Role.VoiceModule is null)
            return VoiceChatChannel.None;
        
        if (receiver == Player && (Flags & VoiceFlags.CanReceiveSelf) != 0)
            messageChannel = VoiceChatChannel.RoundSummary;

        return receiver.Role.VoiceModule.ValidateReceive(Player.Hub, messageChannel);
    }
}