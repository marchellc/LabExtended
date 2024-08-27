using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking
{
    public class TickOptions : ITickOptions
    {
        public volatile TickFlags Flags = TickFlags.None;

        public TickOptions() : this(TickFlags.None) { }

        public TickOptions(TickFlags flags)
            => Flags = flags;

        public bool HasFlag(TickFlags flag)
            => Flags != TickFlags.None && (Flags & flag) == flag;
    }
}