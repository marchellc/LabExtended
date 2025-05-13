using UnityEngine;

namespace LabExtended.API.CustomItems.Properties;

/// <summary>
/// Gets the properties of a Custom Item once dropped.
/// </summary>
public class CustomItemPickupProperties : CustomItemCommonProperties
{
    /// <summary>
    /// Gets or sets the scale of the pickup.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.one;
    
    /// <summary>
    /// Whether or not the pickup should be destroyed when it's owner leaves.
    /// </summary>
    public bool DestroyOnOwnerLeave { get; set; }
}