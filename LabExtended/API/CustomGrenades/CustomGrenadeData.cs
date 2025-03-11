using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomGrenades;

/// <summary>
/// Represents Custom Grenade item configuration.
/// </summary>
public class CustomGrenadeData : CustomItemData
{
    /// <summary>
    /// Gets the grenade's throw force.
    /// </summary>
    public float Force { get; internal set; } = 1f;

    /// <summary>
    /// Gets the grenade's detonation timer.
    /// </summary>
    public float Time { get; internal set; } = -1f;
}