using PlayerRoles;

namespace LabExtended.API.CustomEffects;

public class CustomEffect
{
    public ExPlayer Player { get; internal set; }
    
    public bool IsActive { get; internal set; }
    
    public virtual void Start() { }
    public virtual void Stop() { }
    
    public virtual void ApplyEffects() { }
    public virtual void RemoveEffects() { }
    
    public virtual bool RoleChanged(RoleTypeId newRole) => false;

    internal virtual void OnApplyEffects() => ApplyEffects();
    internal virtual void OnRemoveEffects() => RemoveEffects();
    
    internal virtual bool OnRoleChanged(RoleTypeId newRole) => RoleChanged(newRole);

    public void Enable()
    {
        if (IsActive)
            return;
        
        IsActive = true;
        
        OnApplyEffects();
    }

    public void Disable()
    {
        if (!IsActive)
            return;
        
        IsActive = false;
        
        OnRemoveEffects();
    }
}