using UnityEngine;

namespace LabExtended.API.CustomItems.Properties;

/// <summary>
/// Gets the properties of a Custom Item once dropped.
/// </summary>
public class CustomItemPickupProperties
{
    /// <summary>
    /// Gets or sets the type of the pickup.
    /// </summary>
    public ItemType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the scale of the pickup.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.one;
}