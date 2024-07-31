namespace LabExtended.Commands.Arguments
{
    [Flags]
    public enum ArgumentFlags : byte
    {
        None = 0,

        Optional = 2,
        Remainder = 4
    }
}