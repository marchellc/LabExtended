namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickOptions : TickOptions
    {
        public static UnityTickOptions DefaultOptions => new UnityTickOptions();

        internal int _skippedFrames = 0;
        internal UnityTickLoop _loop;

        public readonly UnityTickFlags Flags = UnityTickFlags.None;
        public readonly int SkipFrames = 0;

        public UnityTickOptions(UnityTickFlags flags, int skipFrames)
        {
            Flags = flags;
            SkipFrames = skipFrames;
        }

        public UnityTickOptions() : this(UnityTickFlags.None, 0) { }
        public UnityTickOptions(int skipFrames) : this(UnityTickFlags.SkipFrames, skipFrames) { }

        public bool HasUnityFlag(UnityTickFlags flag)
            => Flags != UnityTickFlags.None && (Flags & flag) == flag;

        public override string ToString()
            => $"Flags={Flags} SkipFrames={SkipFrames} (SkippedFrames={_skippedFrames})";
    }
}