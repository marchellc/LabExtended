using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Configuration for Custom Firearm items.
/// </summary>
public class CustomFirearmData : CustomItemData
{
    /// <summary>
    /// Gets the firearm's custom ammo type.
    /// </summary>
    public ItemType? AmmoType { get; internal set; }
    
    /// <summary>
    /// Gets the firearm's maximum ammo count.
    /// </summary>
    public ushort? MaxAmmo { get; internal set; }
}