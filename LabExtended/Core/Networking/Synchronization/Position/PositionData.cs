using RelativePositioning;

using UnityEngine;

namespace LabExtended.Core.Networking.Synchronization.Position;

/// <summary>
/// A cache that contains sent player position and rotation.
/// </summary>
public class PositionData
{
    /// <summary>
    /// Gets or sets the last sent position.
    /// </summary>
    public Vector3 Position { get; set; } = default;
    
    /// <summary>
    /// Gets or sets the last sent position.
    /// </summary>
    public RelativePosition RelativePosition { get; set; } = default;

    /// <summary>
    /// Whether or not the position should be force-synced next frame.
    /// </summary>
    public bool IsReset { get; set; } = true;

    /// <summary>
    /// Gets or sets the horizontal axis.
    /// </summary>
    public ushort SyncH { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the vertical axis.
    /// </summary>
    public ushort SyncV { get; set; } = 0;
}