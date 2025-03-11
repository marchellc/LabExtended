using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomUsables;

/// <summary>
/// Custom Usable item configuration.
/// </summary>
public class CustomUsableData : CustomItemData
{
    /// <summary>
    /// Gets the item's cooldown.
    /// </summary>
    public float Cooldown { get; internal set; }
    
    /// <summary>
    /// Gets the item's use time.
    /// </summary>
    public float UseTime { get; internal set; }
}