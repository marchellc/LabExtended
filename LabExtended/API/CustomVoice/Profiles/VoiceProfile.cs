using PlayerRoles;

using VoiceChat.Networking;

namespace LabExtended.API.CustomVoice.Profiles;

public abstract class VoiceProfile
{
    public bool IsActive { get; internal set; }
    
    public ExPlayer Player { get; internal set; }
    
    public virtual void Start() { }
    public virtual void Stop() { }
    
    public virtual void Enable() { }
    public virtual void Disable() { }

    public abstract VoiceProfileResult Receive(ref VoiceMessage message);
    
    public virtual bool OnChangingRole(RoleTypeId newRoleType) => false;
}