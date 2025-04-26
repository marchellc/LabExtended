using InventorySystem.Items.Firearms.Attachments;

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
    /// Gets the firearm's spawn attachments.
    /// </summary>
    public AttachmentName? Attachments { get; internal set; }
    
    /// <summary>
    /// Gets the firearm's base damage.
    /// </summary>
    public float? Damage { get; internal set; }
    
    /// <summary>
    /// Whether or not this firearm can damage teammates.
    /// </summary>
    public bool AllowsTeamDamage { get; internal set; }
    
    /// <summary>
    /// Whether or not this firearm allows attachments to be changed via workstations.
    /// </summary>
    public bool AllowsAttachmentsChange { get; internal set; }
}