namespace LabExtended.Core.Commands
{
    [Flags]
    public enum CommandParsingFlags : byte
    {
        None = 0,
        IgnoreSurplus = 2
    }
}