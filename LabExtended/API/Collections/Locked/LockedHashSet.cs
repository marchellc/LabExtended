using System;
using System.Collections.Generic;

namespace LabExtended.API.Collections.Locked
{
    public class LockedHashSet<T> : LockedCollection<T, HashSet<T>>
    {
        public LockedHashSet() : base(new HashSet<T>()) { }
        public LockedHashSet(HashSet<T> collection) : base(collection) { }
        public LockedHashSet(IEnumerable<T> collection) : base(new HashSet<T>(collection)) { }
        public LockedHashSet(int capacity) : base(new HashSet<T>(capacity)) { }

        public new bool Add(T item)
            => EnterLock(() => Collection.Add(item));

        public int RemoveWhere(Func<T, bool> predicate)
            => EnterLock(() => Collection.RemoveWhere(value => predicate(value)));
    }
}