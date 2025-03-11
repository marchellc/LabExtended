using PlayerRoles;

namespace LabExtended.API.CustomEffects;

/// <summary>
/// Represents a custom player effect.
/// </summary>
public class CustomEffect
{
    /// <summary>
    /// Gets the player that this effect belongs to.
    /// </summary>
    public ExPlayer Player { get; internal set; }
    
    /// <summary>
    /// Whether or not this effect is active.
    /// </summary>
    public bool IsActive { get; internal set; }
    
    /// <summary>
    /// Called when this effect is initially added.
    /// </summary>
    public virtual void Start() { }
    
    /// <summary>
    /// Called when this effect is removed.
    /// </summary>
    public virtual void Stop() { }
    
    /// <summary>
    /// Called when this effect gets applied.
    /// </summary>
    public virtual void ApplyEffects() { }
    
    /// <summary>
    /// Called when this effect gets removed.
    /// </summary>
    public virtual void RemoveEffects() { }

    /// <summary>
    /// Called when the player's role changes.
    /// <para>The returning value determines whether or not to keep the effect active.</para>
    /// <para>Returning TRUE will keep the effect, FALSE will remove it.</para>
    /// </summary>
    /// <param name="newRole">The role the player changed to.</param>
    /// <returns></returns>
    public virtual bool RoleChanged(RoleTypeId newRole) => false;

    /// <summary>
    /// Called when this effect gets enabled (ie when the player changes role while this effect is disabled).
    /// </summary>
    public void Enable()
    {
        if (IsActive)
            return;
        
        IsActive = true;
        
        OnApplyEffects();
    }

    /// <summary>
    /// Called when this effect gets disabled (ie when the player changes role while this effect is enabled).
    /// </summary>
    public void Disable()
    {
        if (!IsActive)
            return;
        
        IsActive = false;
        
        OnRemoveEffects();
    }
    
    internal virtual void OnApplyEffects() => ApplyEffects();
    internal virtual void OnRemoveEffects() => RemoveEffects();
    
    internal virtual bool OnRoleChanged(RoleTypeId newRole) => RoleChanged(newRole);
}