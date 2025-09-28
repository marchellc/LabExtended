using InventorySystem.Items.Firearms.Attachments;

using LabExtended.API.CustomItems.Properties;

namespace LabExtended.API.CustomFirearms.Properties;

/// <summary>
/// Properties of a Custom Firearm while in inventory.
/// </summary>
public class CustomFirearmInventoryProperties : CustomItemInventoryProperties
{
    /// <summary>
    /// Gets the firearm's ammo type.
    /// <remarks>Set to <see cref="ItemType.None"/> if using custom ammo via <see cref="AmmoId"/>!</remarks>
    /// </summary>
    public ItemType AmmoType { get; set; } = ItemType.None;

    /// <summary>
    /// Gets the firearm's custom ammo ID.
    /// </summary>
    public string? AmmoId { get; set; } = null;

    /// <summary>
    /// Gets or sets the firearm's max ammo capacity.
    /// </summary>
    public int? MaxAmmo { get; set; } = null;

    /// <summary>
    /// Gets or sets the maximum nearby ammo range.
    /// </summary>
    public float NearbyAmmoRange { get; set; } = 5f;
    
    /// <summary>
    /// Whether or not ammo that doesn't fit in the player's inventory should be spawned on ground.
    /// <remarks>Only applies if the firearm is using custom ammo that isn't a real ammo type.</remarks>
    /// </summary>
    public bool DropExcessAmmo { get; set; }
    
    /// <summary>
    /// Whether or not the firearm should consume nearby ammo items when reloading.
    /// </summary>
    public bool UseNearbyAmmo { get; set; }

    /// <summary>
    /// Gets the firearm's custom attachments.
    /// </summary>
    public CustomFirearmAttachments Attachments { get; set; } = new();
}