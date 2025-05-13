namespace LabExtended.API.RemoteAdmin.Enums;

/// <summary>
/// Flags for Remote Admin objects.
/// </summary>
[Flags]
public enum RemoteAdminObjectFlags : byte
{
    /// <summary>
    /// No flags were specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// The object should be shown on top of the player list.
    /// </summary>
    ShowOnTop = 1,

    /// <summary>
    /// The object can be shown to Northwood staff members.
    /// </summary>
    ShowToNorthwoodStaff = 2
}