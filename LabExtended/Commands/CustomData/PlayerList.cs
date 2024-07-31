using LabExtended.API;

using System.Collections;

namespace LabExtended.Core.Commands.CustomData
{
    public class PlayerList : IList<ExPlayer>
    {
        private readonly List<ExPlayer> _list;

        public PlayerList(List<ExPlayer> list)
            => _list = list;

        public ExPlayer this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public void Add(ExPlayer item)
            => _list.Contains(item);

        public void Clear()
            => _list.Clear();

        public bool Contains(ExPlayer item)
            => _list.Contains(item);

        public void CopyTo(ExPlayer[] array, int arrayIndex)
            => _list.CopyTo(array, arrayIndex);

        public int IndexOf(ExPlayer item)
            => _list.IndexOf(item);

        public void Insert(int index, ExPlayer item)
            => _list.Insert(index, item);

        public bool Remove(ExPlayer item)
            => _list.Remove(item);

        public void RemoveAt(int index)
            => _list.RemoveAt(index);

        public int RemoveRange(Func<ExPlayer, bool> predicate)
            => _list.RemoveAll(player => predicate(player));

        public IEnumerator<ExPlayer> GetEnumerator()
            => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _list.GetEnumerator();
    }
}