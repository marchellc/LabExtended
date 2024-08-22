using LabExtended.API.Modules;
using LabExtended.API.Pooling;
using LabExtended.API.Voice.Modifiers.Pitch;
using LabExtended.API.Voice.Threading;

using LabExtended.Core.Hooking;
using LabExtended.Core.Ticking;

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

        public override TickTimer TickTimer { get; } = TickTimer.None;

        public volatile float VoicePitch = 1f;

        public bool CanReceiveSelf { get; set; }

        public override void OnStarted()
        {
            base.OnStarted();

            _profiles = ListPool<VoiceProfile>.Shared.Rent();
            _capture = ListPool<byte[]>.Shared.Rent();

            _modifiers = ListPool<VoiceModifier>.Shared.Rent();
            _modifiers.Add(new VoicePitchModifier());
        }

        public override void OnStopped()
        {
            base.OnStopped();

            _profiles.ForEach(x => x.OnDisabled());

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

        public T AddProfile<T>() where T : VoiceProfile
        {
            if (HasProfile<T>(out var profile))
                return profile;

            profile = typeof(T).Construct<T>();

            profile.Owner = CastParent;
            profile.IsEnabled = true;
            profile.OnEnabled();

            _profiles.Add(profile);
            return profile;
        }

        public T AddProfile<T>(T profile) where T : VoiceProfile
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));

            if (HasProfile(out profile))
                return profile;

            if (profile.IsEnabled)
            {
                profile.IsEnabled = false;
                profile.OnDisabled();
                profile.Owner = null;
            }

            profile.Owner = CastParent;
            profile.IsEnabled = true;
            profile.OnEnabled();

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
                profile.IsEnabled = false;
                profile.OnDisabled();
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
                    profile.Owner = null;
                }

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
                profile.IsEnabled = false;
                profile.OnDisabled();
                profile.Owner = null;
            }

            _profiles.Clear();
        }

        public void RemoveModifiers()
            => _modifiers.Clear();

        public override void OnTick()
        {
            base.OnTick();

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

                var channel = GetChannel(player, CastParent, sendChannel);

                if (channel is VoiceChatChannel.None)
                    continue;

                message.Channel = channel;
                player.Connection.Send(message);
            }
        }

        internal void ReceiveThreaded(ThreadedVoicePacket threadedVoicePacket)
        {
            var sendChannel = CastParent.Role.VoiceModule.ValidateSend(threadedVoicePacket.Channel);
            var msg = new VoiceMessage(CastParent.Hub, threadedVoicePacket.Channel, threadedVoicePacket.Data, threadedVoicePacket.Size, false);

            foreach (var modifier in _globalModifiers)
            {
                if (!modifier.IsEnabled || modifier.IsThreaded)
                    continue;

                modifier.ModifySafe(ref msg, this);
            }

            foreach (var modifier in _modifiers)
            {
                if (!modifier.IsEnabled || modifier.IsThreaded)
                    continue;

                modifier.ModifySafe(ref msg, this);
            }

            for (int i = 0; i < ExPlayer._players.Count; i++)
            {
                if (!ExPlayer._players[i])
                    continue;

                var player = ExPlayer._players[i];
                var channel = GetChannel(player, CastParent, sendChannel);

                if (channel is VoiceChatChannel.None)
                    continue;

                msg.Channel = channel;
                player.Connection.Send(msg);
            }

            ObjectPool<ThreadedVoicePacket>.Return(threadedVoicePacket, x => x.Dispose());
        }

        private static VoiceChatChannel GetChannel(ExPlayer receiver, ExPlayer speaker, VoiceChatChannel messageChannel)
        {
            if (receiver.Role.VoiceModule is null)
                return VoiceChatChannel.None;

            if (!speaker.Switches.CanBeHeard
                && (!speaker.Switches.CanBeHeardByOtherRoles || receiver.Role.Type != speaker.Role.Type)
                && (!speaker.Switches.CanBeHeardBySpectators || !receiver.Role.IsSpectator)
                && (!speaker.Switches.CanBeHeardByStaff || !receiver.HasRemoteAdminAccess))
                return VoiceChatChannel.None;

            messageChannel = receiver.Role.VoiceModule.ValidateReceive(speaker.Hub, messageChannel);

            if (receiver == speaker && speaker.Voice.CanReceiveSelf)
                messageChannel = VoiceChatChannel.RoundSummary;

            foreach (var profile in speaker.Voice._profiles)
            {
                if (!profile.IsEnabled)
                    continue;

                profile.ModifyChannel(receiver, ref messageChannel);
            }

            return messageChannel;
        }
    }
}