using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions targeting Firearm attachments.
/// </summary>
public static class FirearmAttachmentExtensions
{
    /// <summary>
    /// Whether or not the firearm has a specific attachment active.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The target attachment.</param>
    /// <returns>true if the attachment is active</returns>
    public static bool HasAttachment(this Firearm firearm, AttachmentName attachmentName)
        => firearm.TryGetAttachment(attachmentName, out var attachment) && attachment.IsEnabled;
    
    /// <summary>
    /// Toggles the status of an attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The target attachment.</param>
    public static void ToggleAttachment(this Firearm firearm, AttachmentName attachmentName)
    {
        if (!firearm.TryGetAttachment(attachmentName, out var attachment))
            return;

        attachment.IsEnabled = !attachment.IsEnabled;
        
        firearm.SyncAttachments();
    }

    /// <summary>
    /// Toggles the status of multiple attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The target attachments list.</param>
    /// <returns>true if any attachments were toggled</returns>
    public static bool ToggleAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
    {
        var anyToggled = false;

        foreach (var attachmentName in attachmentNames)
        {
            if (!firearm.TryGetAttachment(attachmentName, out var attachment))
                continue;

            attachment.IsEnabled = !attachment.IsEnabled;
            anyToggled = true;
        }

        if (anyToggled)
            firearm.SyncAttachments();

        return anyToggled;
    }

    /// <summary>
    /// Sets the status of a specific attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The target attachment.</param>
    /// <param name="enabled">Whether or not the attachment is enabled.</param>
    /// <returns>true if the attachment status was changed</returns>
    public static bool SetAttachment(this Firearm firearm, AttachmentName attachmentName, bool enabled)
    {
        if (!firearm.TryGetAttachment(attachmentName, out var attachment))
            return false;

        if (attachment.IsEnabled == enabled)
            return false;

        attachment.IsEnabled = enabled;
        
        firearm.SyncAttachments();
        return true;
    }

    /// <summary>
    /// Sets the status of multiple attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The target attachments list.</param>
    /// <param name="enabled">Whether or not to enable those attachments.</param>
    /// <returns>true if any attachments were changed</returns>
    public static bool SetAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames, bool enabled)
    {
        var anyChanged = false;

        foreach (var attachmentName in attachmentNames)
        {
            if (!firearm.TryGetAttachment(attachmentName, out var attachment))
                continue;

            if (attachment.IsEnabled == enabled)
                continue;

            attachment.IsEnabled = enabled;
            anyChanged = true;
        }

        if (anyChanged)
            firearm.SyncAttachments();
        
        return anyChanged;
    }

    /// <summary>
    /// Sets random attachments to a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    public static void SetRandomAttachments(this Firearm firearm)
        => firearm.ApplyAttachmentsCode(AttachmentsUtils.GetRandomAttachmentsCode(firearm.ItemTypeId), false);

    /// <summary>
    /// Sets preferred attachments to a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if attachments were changed</returns>
    public static bool SetPreferredAttachments(this Firearm firearm)
    {
        if (firearm.Owner is null)
            return false;

        if (!AttachmentsServerHandler.PlayerPreferences.TryGetValue(firearm.Owner, out var preferences)
            || !preferences.TryGetValue(firearm.ItemTypeId, out var preferenceCode))
            return false;

        firearm.ApplyAttachmentsCode(preferenceCode, false);
        return true;
    }
    
    /// <summary>
    /// Gets all attachments available on a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The list of available attachments.</returns>
    public static IEnumerable<AttachmentName> GetAllAttachments(this Firearm firearm)
        => firearm.Attachments.Select(x => x.Name);
    
    /// <summary>
    /// Gets all enabled attachments on a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The list of enabled attachments.</returns>
    public static IEnumerable<AttachmentName> GetEnabledAttachments(this Firearm firearm)
        => firearm.Attachments.Where(x => x.IsEnabled).Select(x => x.Name);
    
    /// <summary>
    /// Whether or not the target firearm has specific attachments enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to check.</param>
    /// <returns>true if all attachments in <see cref="attachmentNames"/> are enabled</returns>
    public static bool HasAllAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => attachmentNames.All(firearm.HasAttachment);

    /// <summary>
    /// Whether or not the target firearm has specific attachments enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to check.</param>
    /// <returns>true if all attachments in <see cref="attachmentNames"/> are enabled</returns>
    public static bool HasAllAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => attachmentNames.All(firearm.HasAttachment);
    
    /// <summary>
    /// Whether or not the target firearm has any of specified attachments enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to check.</param>
    /// <returns>true if any attachments in <see cref="attachmentNames"/> are enabled</returns>
    public static bool HasAnyAttachment(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => attachmentNames.Any(firearm.HasAttachment);

    /// <summary>
    /// Whether or not the target firearm has any of specified attachments enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to check.</param>
    /// <returns>true if any attachments in <see cref="attachmentNames"/> are enabled</returns>
    public static bool HasAnyAttachment(this Firearm firearm, params AttachmentName[] attachmentNames)
        => attachmentNames.Any(firearm.HasAttachment);

    /// <summary>
    /// Toggles the status of multiple attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The target attachments list.</param>
    /// <returns>true if any attachments were toggled</returns>
    public static void ToggleAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.ToggleAttachments((IEnumerable<AttachmentName>)attachmentNames);

    /// <summary>
    /// Sets the status of multiple attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The target attachments list.</param>
    /// <param name="enabled">Whether or not to enable those attachments.</param>
    /// <returns>true if any attachments were changed</returns>
    public static bool SetAttachments(this Firearm firearm, bool enabled, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, enabled);

    /// <summary>
    /// Enables the specific attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The attachment to enable.</param>
    /// <returns>true if the attachment was enabled</returns>
    public static bool EnableAttachment(this Firearm firearm, AttachmentName attachmentName)
        => firearm.SetAttachment(attachmentName, true);

    /// <summary>
    /// Enables a list of attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to enable.</param>
    /// <returns>true if any of attachments in <see cref="attachmentNames"/> were enabled</returns>
    public static bool EnableAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => firearm.SetAttachments(attachmentNames, true);

    /// <summary>
    /// Enables a list of attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to enable.</param>
    /// <returns>true if any of attachments in <see cref="attachmentNames"/> were enabled</returns>
    public static bool EnableAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, true);

    /// <summary>
    /// Disables the specific attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The attachment to disable.</param>
    /// <returns>true if the attachment was disabled</returns>
    public static bool DisableAttachment(this Firearm firearm, AttachmentName attachmentName)
        => firearm.SetAttachment(attachmentName, false);
    
    /// <summary>
    /// Disables a list of attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to disable.</param>
    /// <returns>true if any of attachments in <see cref="attachmentNames"/> were disabled</returns>
    public static bool DisableAttachments(this Firearm firearm, IEnumerable<AttachmentName> attachmentNames)
        => firearm.SetAttachments(attachmentNames, false);
    
    /// <summary>
    /// Disables a list of attachments.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentNames">The list of attachments to disable.</param>
    /// <returns>true if any of attachments in <see cref="attachmentNames"/> were disabled</returns>
    public static bool DisableAttachments(this Firearm firearm, params AttachmentName[] attachmentNames)
        => firearm.SetAttachments(attachmentNames, false);
    
    /// <summary>
    /// Synchronizes firearm attachments with the client.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    public static void SyncAttachments(this Firearm firearm)
        => firearm.ApplyAttachmentsCode(firearm.GetCurrentAttachmentsCode(), false);
    
    /// <summary>
    /// Gets the firearm's attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The target attachment type.</param>
    /// <returns>found attachment instance, otherwise null</returns>
    public static Attachment? GetAttachment(this Firearm firearm, AttachmentName attachmentName)
        => TryGetAttachment(firearm, attachmentName, out var attachment) ? attachment : null;

    /// <summary>
    /// Gets the firearm's attachment.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="attachmentName">The target attachment type.</param>
    /// <param name="attachment">The found attachment instance.</param>
    /// <returns>true if the attachment was found</returns>
    public static bool TryGetAttachment(this Firearm firearm, AttachmentName attachmentName, out Attachment? attachment)
    {
        attachment = null;

        for (var i = 0; i < firearm.Attachments.Length; i++)
        {
            var curAttachment = firearm.Attachments[i];

            if (curAttachment.Name != attachmentName)
                continue;

            attachment = curAttachment;
            return true;
        }

        return false;
    }
}