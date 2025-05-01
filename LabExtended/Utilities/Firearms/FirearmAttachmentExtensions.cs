using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using LabExtended.Core;
using LabExtended.Extensions;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions targeting Firearm attachments.
/// </summary>
public static class FirearmAttachmentExtensions
{
    /// <summary>
    /// Gets the compiled event delegate used to invoke <see cref="AttachmentsUtils.OnAttachmentsApplied"/>
    /// </summary>
    public static FastEvent<Action<Firearm>> OnAttachmentsApplied { get; } =
        FastEvents.DefineEvent<Action<Firearm>>(typeof(AttachmentsUtils),
            nameof(AttachmentsUtils.OnAttachmentsApplied));

    /// <summary>
    /// Gets enabled attachments from a code.
    /// </summary>
    /// <param name="firearmType">The type of the firearm.</param>
    /// <param name="attachmentsCode">The attachment code.</param>
    /// <param name="attachments">Target list of enabled attachments.</param>
    /// <returns>true if the attachment list was successfully retrieved (false if the item is not a firearm)</returns>
    public static bool GetAttachments(ItemType firearmType, uint attachmentsCode, IList<AttachmentName> attachments)
    {
        if (attachments is null)
            throw new ArgumentNullException(nameof(attachments));
        
        if (!firearmType.TryGetItemPrefab<Firearm>(out var firearm))
            return false;

        var code = 0U;
        
        for (var i = 0; i < firearm.Attachments.Length; i++)
        {
            if ((attachmentsCode & code) == code)
                attachments.Add(firearm.Attachments[i].Name);

            code *= 2U;
        }

        return true;
    }

    /// <summary>
    /// Applies the attachments change to the target firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="toBeEnabled">The attachments to enable.</param>
    /// <param name="toBeDisabled">The attachments to disable.</param>
    /// <param name="invokeEvent">Whether or not to invoke the <see cref="AttachmentsUtils.OnAttachmentsApplied"/> event.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ApplyAttachmentsDiff(this Firearm firearm, IList<AttachmentName> toBeEnabled,
        IList<AttachmentName> toBeDisabled, bool invokeEvent = false)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (toBeEnabled is null)
            throw new ArgumentNullException(nameof(toBeEnabled));

        if (toBeDisabled is null)
            throw new ArgumentNullException(nameof(toBeDisabled));

        var anyChanged = false;

        for (var i = 0; i < firearm.Attachments.Length; i++)
        {
            var attachment = firearm.Attachments[i];

            if (toBeDisabled.Contains(attachment.Name) && attachment.IsEnabled)
            {
                attachment.IsEnabled = false;
                anyChanged = true;
            }
            else if (toBeEnabled.Contains(attachment.Name) && !attachment.IsEnabled)
            {
                attachment.IsEnabled = true;
                anyChanged = true;
            }
        }

        if (anyChanged)
        {
            for (var i = 0; i < firearm.AllSubcomponents.Length; i++)
            {
                if (firearm.AllSubcomponents[i] is FirearmSubcomponentBase subcomponent)
                {
                    subcomponent.OnAttachmentsApplied();
                }
            }

            if (invokeEvent)
                OnAttachmentsApplied.InvokeEvent(null, firearm);

            var firearmCode = firearm.GetCurrentAttachmentsCode();
            
            AttachmentCodeSync.ServerSetCode(firearm.ItemSerial, firearmCode);
            AttachmentsServerHandler.ServerApplyPreference(firearm.Owner, firearm.ItemTypeId, firearmCode);
        }
    }
    
    /// <summary>
    /// Gets a list of active, to-enable and to-disable attachment names.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="newCode">The new validated attachments code.</param>
    /// <param name="current">The list of currently enabled attachments.</param>
    /// <param name="toBeEnabled">The list of attachments to be enabled.</param>
    /// <param name="toBeDisabled">The list of attachments to be disabled.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void GetAttachmentsDiff(this Firearm firearm, uint newCode,
        IList<AttachmentName> current, IList<AttachmentName> toBeEnabled, IList<AttachmentName> toBeDisabled)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (current is null)
            throw new ArgumentNullException(nameof(current));

        if (toBeEnabled is null)
            throw new ArgumentNullException(nameof(toBeEnabled));

        if (toBeDisabled is null)
            throw new ArgumentNullException(nameof(toBeDisabled));

        var code = 1U;

        for (var i = 0; i < firearm.Attachments.Length; i++)
        {
            var attachment = firearm.Attachments[i];
            var enabled = (newCode & code) == code;

            if (attachment.IsEnabled)
                current.Add(attachment.Name);
            
            if (!enabled && attachment.IsEnabled)
                toBeDisabled.Add(attachment.Name);
            else if (enabled && !attachment.IsEnabled)
                toBeEnabled.Add(attachment.Name);

            code *= 2U;
        }
    }
    
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