using LabExtended.API.CustomItems.Properties;

namespace LabExtended.API.CustomUsables.Properties;

/// <summary>
/// Inventory properties of custom usable items.
/// </summary>
public class CustomUsableInventoryProperties : CustomItemInventoryProperties
{
    /// <summary>
    /// Gets or sets the item's use duration (in seconds).
    /// <remarks>Setting this to -1 will force the use of base-game use duration.</remarks>
    /// </summary>
    public float UseDuration { get; set; } = 5f;

    /// <summary>
    /// Gets or sets the item's cooldown duration (in seconds).
    /// </summary>
    public float CooldownDuration { get; set; } = 2f;
}