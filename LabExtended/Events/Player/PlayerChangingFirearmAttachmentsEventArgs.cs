using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;

using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called before a player's firearm attachments are changed.
/// </summary>
public class PlayerChangingFirearmAttachmentsEventArgs : BooleanEventArgs
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
    /// Gets a list of attachments to enable.
    /// </summary>
    public List<AttachmentName> ToEnable { get; }
    
    /// <summary>
    /// Gets a list of attachments to be disabled.
    /// </summary>
    public List<AttachmentName> ToDisable { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerChangingFirearmAttachmentsEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The firearm owner.</param>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="newCode">The received code.</param>
    /// <param name="current">Currently enabled attachments.</param>
    /// <param name="toEnable">Attachments to enable.</param>
    /// <param name="toDisable">Attachments to disable.</param>
    public PlayerChangingFirearmAttachmentsEventArgs(ExPlayer player, Firearm firearm, uint newCode,
        IReadOnlyList<AttachmentName> current, List<AttachmentName> toEnable, List<AttachmentName> toDisable)
    {
        Player = player;
        Firearm = firearm;
        NewCode = newCode;
        Current = current;
        ToEnable = toEnable;
        ToDisable = toDisable;
    }
}