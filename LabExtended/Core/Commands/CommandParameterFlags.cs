namespace LabExtended.Core.Commands
{
    [Flags]
    public enum CommandParameterFlags : byte
    {
        None = 0,
        CatchAll = 2,
        Optional = 4,
    }
}