using System.Collections;

namespace LabExtended.API.Collections.Locked
{
    public class LockedDictionary<TKey, TElement> : IDictionary<TKey, TElement>, IReadOnlyDictionary<TKey, TElement>
    {
        private volatile object _lock;
        private volatile Dictionary<TKey, TElement> _dict;

        public LockedDictionary()
        {
            _lock = new object();
            _dict = new Dictionary<TKey, TElement>();
        }

        public LockedDictionary(int capacity)
        {
            _lock = new object();
            _dict = new Dictionary<TKey, TElement>(capacity);
        }

        public LockedDictionary(IDictionary<TKey, TElement> dictionary)
        {
            _lock = new object();
            _dict = new Dictionary<TKey, TElement>(dictionary);
        }

        public TElement this[TKey key]
        {
            get
            {
                lock (_lock)
                    return _dict[key];
            }
            set
            {
                lock (_lock)
                    _dict[key] = value;
            }
        }

        public ICollection<TKey> Keys => _dict.Keys;
        public ICollection<TElement> Values => _dict.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TElement>.Keys => _dict.Keys;
        IEnumerable<TElement> IReadOnlyDictionary<TKey, TElement>.Values => _dict.Values;

        public int Count => _dict.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TElement value)
        {
            lock (_lock)
                _dict.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TElement> item)
        {
            lock (_lock)
                _dict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            lock (_lock)
                _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TElement> item)
        {
            lock (_lock)
                return _dict.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lock)
                return _dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TElement>[] array, int arrayIndex)
        {
            lock (_lock)
            {
                for (int i = arrayIndex; i < array.Length; i++)
                    array[i] = _dict.ElementAt(i);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TElement>> GetEnumerator()
        {
            lock (_lock)
                return _dict.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            lock (_lock)
                return _dict.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TElement> item)
        {
            lock (_lock)
                return _dict.Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TElement value)
        {
            lock (_lock)
                return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_lock)
                return _dict.GetEnumerator();
        }
    }
}