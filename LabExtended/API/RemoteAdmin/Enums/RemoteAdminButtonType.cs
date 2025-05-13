namespace LabExtended.API.RemoteAdmin.Enums;

/// <summary>
/// Represents the type of a Remote Admin button.
/// </summary>
public enum RemoteAdminButtonType
{
    /// <summary>
    /// A player data request (<b>REQUEST</b> button).
    /// </summary>
    Request = 1,

    /// <summary>
    /// A player IP request (<b>REQUEST IP</b> button).
    /// </summary>
    RequestIp = 0,

    /// <summary>
    /// A player authentification token request (<b>REQUEST AUTH</b> button).
    /// </summary>
    RequestAuth = 3,

    /// <summary>
    /// An external lookup request (<b>EXTERNAL LOOKUP</b> button).
    /// </summary>
    ExternalLookup = -1
}