namespace LabExtended.Utilities.RoleSelection;

[Flags]
public enum RoleSelectorOptions : byte
{
    /// <summary>
    /// No options.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Modify the server's SCP tickets file.
    /// </summary>
    ModifyScpTickets = 1,
    
    /// <summary>
    /// Modify the server's human role history.
    /// </summary>
    ModifyHumanHistory = 2,
    
    /// <summary>
    /// Allows SCP roles to overflow.
    /// </summary>
    AllowScpOverflow = 4
}