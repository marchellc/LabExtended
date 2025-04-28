namespace LabExtended.Core.Networking.Manipulation;

/// <summary>
/// Settings for following.
/// </summary>
public class NetworkObjectFollowSettings
{
    /// <summary>
    /// Gets the following speed.
    /// </summary>
    public float Speed { get; set; } = 30f;

    /// <summary>
    /// Gets the maximum distance before teleporting.
    /// </summary>
    public float MaxDistance { get; set; } = 20f;

    /// <summary>
    /// Gets the minimum distance to follow.
    /// </summary>
    public float MinDistance { get; set; } = 1.75f;
}