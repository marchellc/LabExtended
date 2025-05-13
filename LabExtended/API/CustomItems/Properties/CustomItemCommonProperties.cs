namespace LabExtended.API.CustomItems.Properties;

/// <summary>
/// Base class for properties that are used in inventory and in pickups.
/// </summary>
public class CustomItemCommonProperties
{
    /// <summary>
    /// Gets or sets the type of the item.
    /// </summary>
    public ItemType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the firearm's custom weight (in kilograms).
    /// </summary>
    public float? Weight { get; set; }
}