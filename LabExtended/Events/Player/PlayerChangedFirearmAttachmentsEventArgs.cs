using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;

using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a player's firearm attachments are changed.
/// </summary>
public class PlayerChangedFirearmAttachmentsEventArgs : EventArgs
{
    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the target firearm.
    /// </summary>
    public Firearm Firearm { get; }
    
    /// <summary>
    /// Gets the new attachments code.
    /// </summary>
    public uint NewCode { get; }
    
    /// <summary>
    /// Gets a list of currently enabled attachments.
    /// </summary>
    public IReadOnlyList<AttachmentName> Current { get; }
    
    /// <summary>
    /// Gets a list of attachments that were enabled.
    /// </summary>
    public IReadOnlyList<AttachmentName> EnabledAttachments { get; }
    
    /// <summary>
    /// Gets a list of attachments that were disabled.
    /// </summary>
    public IReadOnlyList<AttachmentName> DisabledAttachments { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerChangedFirearmAttachmentsEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The firearm owner.</param>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="newCode">The received code.</param>
    /// <param name="current">Currently enabled attachments.</param>
    /// <param name="enabled">Enabled attachments.</param>
    /// <param name="disabled">Disabled attachments.</param>
    public PlayerChangedFirearmAttachmentsEventArgs(ExPlayer player, Firearm firearm, uint newCode,
        IReadOnlyList<AttachmentName> current, IReadOnlyList<AttachmentName> enabled, IReadOnlyList<AttachmentName> disabled)
    {
        Player = player;
        Firearm = firearm;
        NewCode = newCode;
        Current = current;
        EnabledAttachments = enabled;
        DisabledAttachments = disabled;
    }
}