using RelativePositioning;

namespace LabExtended.API.PositionSync;

/// <summary>
/// Caches sent positions to other players.
/// </summary>
public class PositionCache
{
    /// <summary>
    /// Gets or sets the horizontal mouse axis.
    /// </summary>
    public ushort LookHorizontal { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the vertical mouse axis.
    /// </summary>
    public ushort LookVertical { get; set; } = 0;

    /// <summary>
    /// Whether or not the cached position is invalid.
    /// </summary>
    public bool IsDirty { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the previous previous.
    /// </summary>
    public RelativePosition Position { get; set; }
}