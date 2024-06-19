using PlayerRoles;

using VoiceChat;

namespace LabExtended.API.Voice.Profiles
{
    public class VoiceProfileBase
    {
        public ExPlayer Player { get; internal set; }
        public bool IsActive { get; internal set; }

        public virtual void OnStarted() { }
        public virtual void OnStopped() { }

        public virtual void OnReceived(Dictionary<ExPlayer, VoiceChatChannel> list, Action<ExPlayer, VoiceChatChannel> edit) { }

        public virtual void OnStartedSpeaking() { }
        public virtual void OnStoppedSpeaking(DateTime startedAt, TimeSpan speakingDuration, byte[][] capture) { }

        public virtual bool ShouldKeep(RoleTypeId newRole) => false;
    }
}