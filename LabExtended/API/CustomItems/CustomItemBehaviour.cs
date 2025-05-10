namespace LabExtended.API.CustomItems;

/// <summary>
/// Represents the base class for a behaviour component of a Custom Item.
/// </summary>
public class CustomItemBehaviour
{
    /// <summary>
    /// Whether or not the behaviour is enabled.
    /// </summary>
    public bool IsEnabled { get; internal set; }
    
    /// <summary>
    /// Gets the behaviour's handler.
    /// </summary>
    public CustomItemHandler? Handler { get; internal set; }
    
    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public virtual void OnUpdate() { }
    
    /// <summary>
    /// Gets called once the behaviour is enabled.
    /// </summary>
    public virtual void OnEnabled() { }
    
    /// <summary>
    /// Gets called once the behaviour is disabled.
    /// </summary>
    public virtual void OnDisabled() { }
}