namespace LabExtended.API.RemoteAdmin.Enums
{
    [Flags]
    public enum RemoteAdminObjectFlags : byte
    {
        None = 0,

        ShowOnTop = 2,
        ShowToNorthwoodStaff = 4
    }
}