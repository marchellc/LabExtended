using System.Collections.Generic;

namespace LabExtended.API.Collections.Locked
{
    public class LockedSortedSet<T> : LockedCollection<T, SortedSet<T>>
    {
        public LockedSortedSet() : base(new SortedSet<T>()) { }
        public LockedSortedSet(SortedSet<T> collection) : base(collection) { }
        public LockedSortedSet(IEnumerable<T> elements) : base(new SortedSet<T>(elements)) { }
        public LockedSortedSet(IComparer<T> comparer) : base(new SortedSet<T>(comparer)) { }
        public LockedSortedSet(IComparer<T> comparer, IEnumerable<T> elements) : base(new SortedSet<T>(elements, comparer)) { }
    }
}