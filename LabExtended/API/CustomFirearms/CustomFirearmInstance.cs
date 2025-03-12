using InventorySystem.Items.Firearms;

using LabExtended.API.CustomItems;

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Represents an active Custom Firearm item instance.
/// </summary>
public class CustomFirearmInstance : CustomItemInstance
{
    /// <summary>
    /// Gets the associated Firearm item.
    /// </summary>
    public new Firearm? Item => base.Item as Firearm;
    
    /// <summary>
    /// Gets the associated Firearm pickup.
    /// </summary>
    public new FirearmPickup? Pickup => base.Pickup as FirearmPickup;
    
    /// <summary>
    /// Gets the Custom Firearm configuration.
    /// </summary>
    public new CustomFirearmData? CustomData => base.CustomData as CustomFirearmData;
}