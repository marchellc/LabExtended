using LabExtended.Utilities.Unity;

namespace LabExtended.API.CustomRoles;

/// <summary>
/// A class used for controlling custom role instances.
/// </summary>
public class CustomRoleInstance : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether or not this custom role is active.
    /// </summary>
    public bool IsActive { get; internal set; }
    
    /// <summary>
    /// Gets the role's owner.
    /// </summary>
    public ExPlayer? Owner { get; internal set; }
    
    /// <summary>
    /// Gets the role's data.
    /// </summary>
    public CustomRoleData? CustomData { get; internal set; }
    
    /// <summary>
    /// Gets called once an instance of this role is made and configured.
    /// </summary>
    public virtual void OnInstantiated() { }
    
    /// <summary>
    /// Gets called once this role gets enabled (the player gets this role granted).
    /// </summary>
    public virtual void OnEnabled() { }
    
    /// <summary>
    /// Gets called once this role gets disabled (the player changes role OR the role gets removed).
    /// </summary>
    public virtual void OnDisabled() { }
    
    /// <summary>
    /// Gets called each frame (when active).
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Clears this role's data, gets called once the owning player leaves (or this role gets removed).
    /// </summary>
    public virtual void Dispose()
    {
        PlayerLoopHelper.AfterLoop -= InvokeUpdate;
    }

    internal void InvokeInstantiated()
    {
        PlayerLoopHelper.AfterLoop += InvokeUpdate;
    }

    private void InvokeUpdate()
    {
        if (!IsActive || !Owner)
            return;
        
        OnUpdate();
    }
}