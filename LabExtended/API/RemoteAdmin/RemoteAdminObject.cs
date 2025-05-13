using LabExtended.API.Enums;

using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.RemoteAdmin;

/// <summary>
/// Base class for Remote Admin objects.
/// </summary>
public class RemoteAdminObject : IRemoteAdminObject
{
    /// <inheritdoc cref="IRemoteAdminObject.Flags"/>
    public virtual RemoteAdminObjectFlags Flags { get; } = RemoteAdminObjectFlags.ShowToNorthwoodStaff;
    
    /// <inheritdoc cref="IRemoteAdminObject.Icons"/>
    public virtual RemoteAdminIconType Icons { get; } = RemoteAdminIconType.None;

    /// <inheritdoc cref="IRemoteAdminObject.CustomId"/>
    public virtual string CustomId { get; set; }

    /// <inheritdoc cref="IRemoteAdminObject.Id"/>
    public string Id { get; set; }
    
    /// <inheritdoc cref="IRemoteAdminObject.ListId"/>
    public int ListId { get; set; }
    
    /// <inheritdoc cref="IRemoteAdminObject.IsActive"/>
    public bool IsActive { get; set; }

    /// <inheritdoc cref="IRemoteAdminObject.GetName"/>
    public virtual string GetName(ExPlayer player)
        => string.Empty;

    /// <inheritdoc cref="IRemoteAdminObject.GetResponse"/>
    public virtual string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers,
        RemoteAdminButtonType button)
        => string.Empty;

    /// <inheritdoc cref="IRemoteAdminObject.GetButton"/>
    public virtual string GetButton(ExPlayer player, RemoteAdminButtonType buttonType)
        => string.Empty;

    /// <inheritdoc cref="IRemoteAdminObject.GetVisibility"/>
    public virtual bool GetVisibility(ExPlayer player)
        => true;

    /// <inheritdoc cref="IRemoteAdminObject.OnDisabled"/>
    public virtual void OnDisabled() { }

    /// <inheritdoc cref="IRemoteAdminObject.OnEnabled"/>
    public virtual void OnEnabled() { }
}