using InventorySystem.Items.Firearms;

using LabExtended.API.CustomItems.Behaviours;

#pragma warning disable CS8603 // Possible null reference return.

namespace LabExtended.API.CustomFirearms.Behaviours;

/// <summary>
/// The behaviour of a dropped custom firearm.
/// </summary>
public class CustomFirearmPickupBehaviour : CustomItemPickupBehaviour
{
    /// <summary>
    /// Gets the firearm pickup.
    /// </summary>
    public new FirearmPickup Pickup => base.Pickup as FirearmPickup;
    
    /// <summary>
    /// Gets the custom firearm handler.
    /// </summary>
    public new CustomFirearmHandler Handler => base.Handler as CustomFirearmHandler;
}