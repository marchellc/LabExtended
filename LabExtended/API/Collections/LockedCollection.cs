using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace LabExtended.API.Collections
{
    public class LockedCollection<TElement, TCollection> :
        ICollection<TElement>,

        IReadOnlyCollection<TElement>,
        IReadOnlyList<TElement>

        where TCollection : class, ICollection<TElement>
    {
        private volatile TCollection _collection;
        private volatile IList<TElement> _list;
        private volatile TElement[] _array;
        private volatile object _lock;

        public LockedCollection(TCollection collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            _collection = collection;

            _list = collection as IList<TElement>;
            _array = collection as TElement[];

            _lock = new object();
        }

        public TCollection Collection
        {
            get
            {
                lock (_lock)
                    return _collection;
            }
        }

        public TElement this[int index]
        {
            get
            {
                lock (_lock)
                {
                    if (_list != null)
                        return _list[index];
                    else if (_array != null)
                        return _array[index];
                    else
                        return _collection.ElementAt(index);
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_list != null)
                        _list[index] = value;
                    else if (_array != null)
                        _array[index] = value;
                    else
                        throw new InvalidOperationException($"This collection type ({typeof(TCollection).FullName}) does not support indexers.");
                }
            }
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => false;

        public virtual void Add(TElement item)
        {
            lock (_lock)
                _collection.Add(item);
        }

        public virtual bool AddIfNotContains(TElement item)
        {
            lock (_lock)
            {
                if (!_collection.Contains(item))
                {
                    _collection.Add(item);
                    return true;
                }

                return false;
            }
        }

        public virtual bool AddIfNotContains(TElement item, Func<TElement, TElement, bool> comparer)
        {
            lock (_lock)
            {
                if (!_collection.Any(element => comparer(element, item)))
                {
                    _collection.Add(item);
                    return true;
                }

                return false;
            }
        }

        public virtual bool AddIfNotContains(TElement item, IComparer<TElement> comparer)
        {
            lock (_lock)
            {
                if (!_collection.Any(element => comparer.Compare(element, item) == 1))
                {
                    _collection.Add(item);
                    return true;
                }

                return false;
            }
        }

        public virtual void AddRange(IEnumerable<TElement> elements)
        {
            lock (_lock)
            {
                foreach (var element in elements)
                    _collection.Add(element);
            }
        }

        public virtual void AddRangeIfNotContains(IEnumerable<TElement> elements)
        {
            lock (_lock)
            {
                foreach (var element in elements)
                {
                    if (!Contains(element))
                        _collection.Add(element);
                }
            }
        }

        public virtual void AddRangeIfNotContains(IEnumerable<TElement> elements, Func<TElement, TElement, bool> comparer)
        {
            lock (_lock)
            {
                foreach (var element in elements)
                {
                    if (!Contains(element, comparer))
                        _collection.Add(element);
                }
            }
        }

        public virtual void AddRangeIfNotContains(IEnumerable<TElement> elements, IComparer<TElement> comparer)
        {
            lock (_lock)
            {
                foreach (var element in elements)
                {
                    if (!Contains(element, comparer))
                        _collection.Add(element);
                }
            }
        }

        public virtual void Clear()
        {
            lock (_lock)
                _collection.Clear();
        }

        public virtual bool Contains(TElement item)
        {
            lock (_lock)
                return _collection.Contains(item);
        }

        public virtual bool Contains(TElement item, IComparer<TElement> comparer)
        {
            lock (_lock)
                return _collection.Any(element => comparer.Compare(element, item) == 1);
        }

        public virtual bool Contains(TElement item, Func<TElement, TElement, bool> comparer)
        {
            lock (_lock)
                return _collection.Any(element => comparer(element, item));
        }

        public virtual bool Remove(TElement item)
        {
            lock (_lock)
                return _collection.Remove(item);
        }

        public virtual int RemoveAll(Func<TElement, bool> comparer)
        {
            lock (_lock)
            {
                var nextIndex = -1;
                var removed = 0;

                while ((nextIndex = FindIndex(comparer)) >= 0)
                {
                    _collection.Remove(_collection.ElementAt(nextIndex));
                    removed++;
                }

                return removed;
            }
        }

        public virtual int FindIndex(Func<TElement, bool> comparer)
        {
            lock (_lock)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (comparer(_collection.ElementAt(i)))
                        return i;
                }

                return -1;
            }
        }

        public virtual void CopyTo(TElement[] array, int arrayIndex)
        {
            lock (_lock)
                _collection.CopyTo(array, arrayIndex);
        }

        public virtual void CopyTo(ICollection<TElement> elements, int startIndex = 0)
        {
            lock (_lock)
            {
                for (int i = startIndex; i < Count; i++)
                    elements.Add(_collection.ElementAt(i));
            }
        }

        public virtual IEnumerator<TElement> GetEnumerator()
        {
            lock (_lock)
                return _collection.GetEnumerator();
        }

        public bool TryGetFirst(Func<TElement, bool> predicate, out TElement element)
        {
            foreach (var item in _collection)
            {
                if (predicate(item))
                {
                    element = item;
                    return true;
                }
            }

            element = default;
            return false;
        }

        public void EnterLock(Action action)
        {
            lock (_lock)
                action();
        }

        public T EnterLock<T>(Func<T> func)
        {
            lock (_lock)
                return func();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}