using PlayerRoles;

using VoiceChat.Networking;

namespace LabExtended.API.Voice
{
    public abstract class VoiceProfile
    {
        public bool IsEnabled { get; internal set; }

        public ExPlayer Owner { get; internal set; }

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        
        public virtual void OnStart() { }
        public virtual void OnDestroy() { }

        public virtual bool OnRoleChanged(RoleTypeId newRole) => false;

        public abstract bool TryReceive(ExPlayer receiver, ref VoiceMessage message);
    }
}