namespace LabExtended.Events;

/// <summary>
/// An event with a IsAllowed property.
/// </summary>
public class BooleanEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new BooleanEventArgs instance with <see cref="IsAllowed"/> set to true.
    /// </summary>
    public BooleanEventArgs()
        => IsAllowed = true;
    
    /// <summary>
    /// Creates a new BooleanEventArgs instance.
    /// </summary>
    /// <param name="isAllowed">Whether or not the event should be allowed.</param>
    public BooleanEventArgs(bool isAllowed = true)
        => IsAllowed = isAllowed;
    
    /// <summary>
    /// Whether or not the event should be allowed.
    /// </summary>
    public bool IsAllowed { get; set; }
}