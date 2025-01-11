using LabExtended.API.Modules;
using LabExtended.API.Voice.Modifiers.Pitch;
using LabExtended.API.Voice.Threading;

using LabExtended.Core.Hooking;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using VoiceChat;
using VoiceChat.Networking;

namespace LabExtended.API.Voice
{
    public class VoiceModule : GenericModule<ExPlayer>
    {
        public static event Action<ExPlayer> OnStartedSpeaking;
        public static event Action<ExPlayer, DateTime, TimeSpan, IReadOnlyList<byte[]>> OnStoppedSpeaking;

        private static volatile List<VoiceModifier> _globalModifiers = ListPool<VoiceModifier>.Shared.Rent();

        public static volatile float GlobalVoicePitch = 1f;

        private volatile List<VoiceModifier> _modifiers;
        private volatile List<VoiceProfile> _profiles;
        private volatile List<byte[]> _capture;

        private bool _speaking;
        private DateTime _speakingStart;

        public IReadOnlyList<VoiceModifier> Modifiers => _modifiers;
        public IReadOnlyList<VoiceProfile> Profiles => _profiles;

        public static IReadOnlyCollection<VoiceModifier> GlobalModifiers => _globalModifiers;

        public volatile float VoicePitch = 1f;

        public bool CanReceiveSelf { get; set; }

        public override void OnStarted()
        {
            base.OnStarted();

            _profiles = ListPool<VoiceProfile>.Shared.Rent();
            _capture = ListPool<byte[]>.Shared.Rent();

            _modifiers = ListPool<VoiceModifier>.Shared.Rent();
            _modifiers.Add(new VoicePitchModifier());

            InternalEvents.OnSpawning += OnSpawning;
        }

        public override void OnStopped()
        {
            base.OnStopped();

            InternalEvents.OnSpawning -= OnSpawning;
            
            _profiles.ForEach(x => x.OnDestroy());

            _modifiers.ForEach(x =>
            {
                if (x is not IDisposable disposable)
                    return;

                disposable.Dispose();
            });

            ListPool<VoiceModifier>.Shared.Return(_modifiers);
            ListPool<VoiceProfile>.Shared.Return(_profiles);
            ListPool<byte[]>.Shared.Return(_capture);

            _modifiers = null;
            _profiles = null;
            _capture = null;
        }

        public bool HasModifier<T>() where T : VoiceModifier
            => _modifiers.Any(m => m.IsEnabled && m is T);

        public bool HasModifier<T>(out T modifier) where T : VoiceModifier
            => (_modifiers.TryGetFirst<VoiceModifier>(m => m.IsEnabled && m is T, out var voiceModifier) ? modifier = (T)voiceModifier : modifier = null) != null;

        public bool HasProfile<T>() where T : VoiceProfile
            => _profiles.Any(p => p.IsEnabled && p is T);

        public bool HasProfile<T>(out T profile) where T : VoiceProfile
            => (_profiles.TryGetFirst<VoiceProfile>(p => p.IsEnabled && p is T, out var voiceProfile) ? profile = (T)voiceProfile : profile = null) != null;

        public T AddProfile<T>(bool enableProfile = false) where T : VoiceProfile
        {
            if (HasProfile<T>(out var profile))
                return profile;

            profile = typeof(T).Construct<T>();

            profile.Owner = CastParent;
            profile.OnStart();

            if (enableProfile)
            {
                profile.IsEnabled = true;
                profile.OnEnabled();
            }

            _profiles.Add(profile);
            return profile;
        }

        public T AddModifier<T>() where T : VoiceModifier
        {
            if (HasModifier<T>(out var modifier))
                return modifier;

            modifier = typeof(T).Construct<T>();

            _modifiers.Add(modifier);
            return modifier;
        }

        public T GetProfile<T>() where T : VoiceProfile
        {
            if (HasProfile<T>(out var profile))
                return profile;

            return default;
        }

        public T GetModifier<T>() where T : VoiceModifier
        {
            if (HasModifier<T>(out var modifier))
                return modifier;

            return default;
        }

        public bool RemoveProfile<T>() where T : VoiceProfile
        {
            if (HasProfile<T>(out var profile))
            {
                if (profile.IsEnabled)
                {
                    profile.IsEnabled = false;
                    profile.OnDisabled();
                }
                
                profile.OnDestroy();
                profile.Owner = null;

                _profiles.Remove(profile);
                return true;
            }

            return false;
        }

        public bool RemoveProfile<T>(T profile) where T : VoiceProfile
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));

            if (_profiles.Contains(profile))
            {
                if (profile.IsEnabled)
                {
                    profile.IsEnabled = false;
                    profile.OnDisabled();
                }

                profile.OnDestroy();
                profile.Owner = null;

                _profiles.Remove(profile);
                return true;
            }

            return false;
        }

        public bool RemoveModifier<T>() where T : VoiceModifier
        {
            if (HasModifier<T>(out var modifier))
                return _modifiers.Remove(modifier);

            return false;
        }

        public void RemoveProfiles()
        {
            foreach (var profile in _profiles)
            {
                if (profile.IsEnabled)
                {
                    profile.IsEnabled = false;
                    profile.OnDisabled();
                }

                profile.OnDestroy();
                profile.Owner = null;
            }

            _profiles.Clear();
        }

        public void RemoveModifiers()
            => _modifiers.Clear();

        public override void Update()
        {
            if (CastParent is null)
                return;

            if (CastParent.IsSpeaking && !_speaking)
            {
                _speaking = true;
                _speakingStart = DateTime.Now;

                _capture.Clear();

                OnStartedSpeaking?.Invoke(CastParent);
                
                HookRunner.RunEvent(new PlayerStartedSpeakingArgs(CastParent));
            }
            else if (!CastParent.IsSpeaking && _speaking)
            {
                _speaking = false;
                _speakingStart = DateTime.MinValue;

                OnStoppedSpeaking?.Invoke(CastParent, _speakingStart, DateTime.Now - _speakingStart, _capture);
                
                HookRunner.RunEvent(new PlayerStoppedSpeakingArgs(CastParent, _speakingStart, DateTime.Now - _speakingStart, _capture));

                _capture.Clear();
            }
        }

        internal void ReceiveMessage(ref VoiceMessage message)
        {
            if (message.SpeakerNull || message.Speaker != CastParent.Hub)
                return;

            foreach (var modifier in _globalModifiers)
            {
                if (!modifier.IsEnabled || modifier.IsThreaded)
                    continue;

                modifier.ModifySafe(ref message, this);
            }

            foreach (var modifier in _modifiers)
            {
                if (!modifier.IsEnabled || modifier.IsThreaded)
                    continue;

                modifier.ModifySafe(ref message, this);
            }

            var sendChannel = CastParent.Role.VoiceModule.ValidateSend(message.Channel);

            for (int i = 0; i < ExPlayer._players.Count; i++)
            {
                var player = ExPlayer._players[i];

                if (player is null || !player)
                    continue;

                var channel = GetChannel(player, sendChannel);
                var status = true;
                
                if (channel is VoiceChatChannel.None)
                    continue;

                message.Channel = channel;

                for (int x = 0; x < _profiles.Count; x++)
                {
                    var profile = _profiles[x];
                    
                    if (profile is null || !profile.IsEnabled)
                        continue;

                    if (!profile.TryReceive(player, ref message))
                    {
                        status = false;
                        break;
                    }
                }
                
                if (!status)
                    continue;
                
                player.Connection.Send(message);
            }
        }
        
        internal void ReceiveThreaded(ThreadedVoicePacket threadedVoicePacket)
        {
            var sendChannel = CastParent.Role.VoiceModule.ValidateSend(threadedVoicePacket.Channel);
            var buffer = default(byte[]);

            ThreadedVoiceChat.Copy(ref threadedVoicePacket.Size, ref threadedVoicePacket.Data, ref buffer);

            var msg = new VoiceMessage(CastParent.Hub, threadedVoicePacket.Channel, buffer, threadedVoicePacket.Size, false);

            for (int i = 0; i < ExPlayer._players.Count; i++)
            {
                if (!ExPlayer._players[i])
                    continue;

                var player = ExPlayer._players[i];
                var channel = GetChannel(player, sendChannel);
                var status = true;

                if (channel is VoiceChatChannel.None)
                    continue;

                msg.Channel = channel;

                for (int x = 0; x < _profiles.Count; x++)
                {
                    var profile = _profiles[x];
                    
                    if (profile is null || !profile.IsEnabled)
                        continue;

                    if (!profile.TryReceive(player, ref msg))
                    {
                        status = false;
                        break;
                    }
                }
                
                if (!status)
                    continue;
                
                player.Connection.Send(msg);
            }

            threadedVoicePacket.Dispose();
        }
        
        private VoiceChatChannel GetChannel(ExPlayer receiver, VoiceChatChannel messageChannel)
        {
            if (receiver.Role.VoiceModule is null)
                return VoiceChatChannel.None;
            
            if (receiver == CastParent && CanReceiveSelf)
                messageChannel = VoiceChatChannel.RoundSummary;

            return messageChannel;
        }

        private void OnSpawning(PlayerSpawningArgs args)
        {
            if (args.Player != CastParent)
                return;

            foreach (var profile in Profiles)
            {
                if (!profile.OnRoleChanged(args.NewRole))
                {
                    if (profile.IsEnabled)
                    {
                        profile.IsEnabled = false;
                        profile.OnDisabled();
                    }
                }
                else
                {
                    if (!profile.IsEnabled)
                    {
                        profile.OnEnabled();
                        profile.IsEnabled = true;
                    }
                }
            }
        }
    }
}