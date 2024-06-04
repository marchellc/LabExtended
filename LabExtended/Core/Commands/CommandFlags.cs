namespace LabExtended.Core.Commands
{
    public enum CommandFlags : byte
    {
        None = 0,

        EnableRemoteAdmin = 2,
        EnableServerConsole = 4,
        EnablePlayerConsole = 8,

        DisableServerPlayer = 16
    }
}