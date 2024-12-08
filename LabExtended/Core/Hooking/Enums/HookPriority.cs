namespace LabExtended.Core.Hooking.Enums
{
    public enum HookPriority : short
    {
        AlwaysLast = 0,

        Lowest = 2,
        Low = 4,

        Medium = 8,

        Normal = 16,

        High = 32,
        Higher = 64,
        Highest = 128,

        AlwaysFirst = 256
    }
}