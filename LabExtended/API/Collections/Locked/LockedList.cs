using System.Collections.Generic;

namespace LabExtended.API.Collections.Locked
{
    public class LockedList<T> : LockedCollection<T, List<T>>, IList<T>
    {
        public LockedList() : base(new List<T>()) { }
        public LockedList(List<T> collection) : base(collection) { }
        public LockedList(IEnumerable<T> elements) : base(new List<T>(elements)) { }
        public LockedList(int capacity) : base(new List<T>(capacity)) { }

        public int IndexOf(T item)
            => Collection.IndexOf(item);

        public void Insert(int index, T item)
            => Collection.Insert(index, item);

        public void RemoveAt(int index)
            => Collection.RemoveAt(index);
    }
}