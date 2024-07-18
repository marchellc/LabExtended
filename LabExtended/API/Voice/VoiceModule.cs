using LabExtended.API.Collections.Locked;
using LabExtended.API.Modules;
using LabExtended.API.Voice.Modifiers.Pitch;

using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;
using LabExtended.Core.Ticking;

using VoiceChat;
using VoiceChat.Networking;

namespace LabExtended.API.Voice
{
    public class VoiceModule : GenericModule<ExPlayer>
    {
        public static event Action<ExPlayer> OnStartedSpeaking;
        public static event Action<ExPlayer, DateTime, TimeSpan, IReadOnlyList<byte[]>> OnStoppedSpeaking;

        private static readonly LockedHashSet<VoiceModifier> _globalModifiers = new LockedHashSet<VoiceModifier>();

        public static float GlobalVoicePitch { get; set; } = 1f;

        private readonly List<VoiceModifier> _modifiers = new List<VoiceModifier>() { new VoicePitchModifier() };
        private readonly List<VoiceProfile> _profiles = new List<VoiceProfile>();
        private readonly List<byte[]> _capture = new List<byte[]>();

        private bool _speaking;
        private DateTime _speakingStart;

        public IReadOnlyList<VoiceModifier> Modifiers => _modifiers;
        public IReadOnlyList<VoiceProfile> Profiles => _profiles;

        public override TickTimer TickTimer { get; } = TickTimer.None;

        public float VoicePitch { get; set; } = 1f;

        public bool CanReceiveSelf { get; set; }

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
                if (!modifier.IsEnabled)
                    continue;

                modifier.Modify(ref message, this);
            }

            foreach (var modifier in _modifiers)
            {
                if (!modifier.IsEnabled)
                    continue;

                modifier.Modify(ref message, this);
            }

            var sendChannel = CastParent.Role.VoiceModule.ValidateSend(message.Channel);

            foreach (var player in ExPlayer.Players)
            {
                var channel = GetChannel(player, sendChannel);

                if (channel is VoiceChatChannel.None)
                    continue;

                message.Channel = channel;
                player.Connection.Send(message);
            }
        }

        private VoiceChatChannel GetChannel(ExPlayer receiver, VoiceChatChannel messageChannel)
        {
            if (receiver.Role.VoiceModule is null)
                return VoiceChatChannel.None;

            messageChannel = receiver.Role.VoiceModule.ValidateReceive(CastParent.Hub, messageChannel);

            if (receiver == CastParent && CanReceiveSelf)
                messageChannel = VoiceChatChannel.RoundSummary;

            foreach (var profile in _profiles)
            {
                if (!profile.IsEnabled)
                    continue;

                profile.ModifyChannel(receiver, ref messageChannel);
            }

            return messageChannel;
        }
    }
}