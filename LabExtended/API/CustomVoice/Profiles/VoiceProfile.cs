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

    public abstract VoiceProfileResult ReceiveFrom(ref VoiceMessage message);
    public abstract VoiceProfileResult SendTo(ref VoiceMessage message, ExPlayer player);
    
    public virtual bool EnabledOnRoleChange(RoleTypeId newRoleType) => false;
}