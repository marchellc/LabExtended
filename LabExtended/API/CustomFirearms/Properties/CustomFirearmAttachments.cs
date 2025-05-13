using InventorySystem.Items.Firearms.Attachments;

namespace LabExtended.API.CustomFirearms.Properties;

/// <summary>
/// Attachments of a Custom Firearm.
/// </summary>
public class CustomFirearmAttachments
{
    /// <summary>
    /// Gets a list of default attachments that the firearm will spawn with.
    /// </summary>
    public List<AttachmentName> DefaultAttachments { get; set; } = new();
    
    /// <summary>
    /// Gets a list of whitelisted attachments.
    /// </summary>
    public List<AttachmentName> WhitelistedAttachments { get; set; } = new();
    
    /// <summary>
    /// Gets a list of blacklisted attachments.
    /// </summary>
    public List<AttachmentName> BlacklistedAttachments { get; set; } = new();
}