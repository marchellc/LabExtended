using PlayerRoles;

using VoiceChat;

namespace LabExtended.API.Voice
{
    public abstract class VoiceProfile
    {
        public bool IsEnabled { get; internal set; }

        public ExPlayer Owner { get; internal set; }

        public abstract void ModifyChannel(ExPlayer receiver, ref VoiceChatChannel receiverChannel);

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }

        public virtual bool OnRoleChanged(RoleTypeId newRole) => false;
    }
}