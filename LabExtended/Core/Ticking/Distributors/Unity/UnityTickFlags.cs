namespace LabExtended.Core.Ticking.Distributors.Unity
{
    [Flags]
    public enum UnityTickFlags : byte
    {
        None = 0,
        SkipFrames = 2
    }
}