namespace LabExtended.Core.Ticking.Distributors.Unity
{
    public class UnityTickOptions : TickOptions
    {
        public static UnityTickOptions DefaultOptions => new UnityTickOptions();

        internal int _skippedFrames = 0;
        internal UnityTickComponent _separateComponent;

        public readonly UnityTickFlags Flags = UnityTickFlags.None;
        public readonly UnityTickSegment Segment = UnityTickSegment.Update;

        public readonly int SkipFrames = 0;

        public UnityTickOptions(UnityTickFlags flags, UnityTickSegment segment, int skipFrames)
        {
            Flags = flags;
            Segment = segment;
            SkipFrames = skipFrames;
        }

        public UnityTickOptions() : this(UnityTickFlags.None, UnityTickSegment.Update, 0) { }
        public UnityTickOptions(UnityTickSegment segment) : this(UnityTickFlags.None, segment, 0) { }

        public UnityTickOptions(int skipFrames) : this(UnityTickFlags.SkipFrames, UnityTickSegment.Update, skipFrames) { }
        public UnityTickOptions(int skipFrames, UnityTickSegment segment) : this(UnityTickFlags.SkipFrames, segment, skipFrames) { }

        public bool HasUnityFlag(UnityTickFlags flag)
            => Flags != UnityTickFlags.None && (Flags & flag) == flag;
    }
}