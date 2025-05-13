using LabExtended.API.CustomItems.Properties;

namespace LabExtended.API.CustomFirearms.Properties;

/// <summary>
/// Properties of a Custom Firearm while spawned.
/// </summary>
public class CustomFirearmPickupProperties : CustomItemPickupProperties
{
    /// <summary>
    /// Gets the firearm's custom attachments.
    /// </summary>
    public CustomFirearmAttachments attachments { get; set; } = new();
}