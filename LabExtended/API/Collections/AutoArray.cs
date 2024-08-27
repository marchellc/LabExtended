using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.API.Collections
{
    public class AutoArray<T> where T : class
    {
        private volatile int _size = 0;
        private volatile int _free = 0;
        private volatile int _last = -1;

        public volatile T[] ValueArray;
        public volatile int ResizeCount;

        public int Capacity => ValueArray.Length;

        public int Size => _size;
        public int Free => _free;

        public bool IsValid => ValueArray != null && ResizeCount > 0;

        public bool IsEmpty => _size == 0;
        public bool IsFull => _free == 0;

        public AutoArray() : this(100, 50) { }
        public AutoArray(int capacity) : this(capacity, capacity > 10 ? capacity / 2 : capacity * 2) { }

        public AutoArray(int capacity, int resize)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            if (resize < 0)
                throw new ArgumentOutOfRangeException(nameof(resize));

            ResizeCount = resize;

            _free = capacity;

            _size = 0;
            _last = -1;

            ValueArray = new T[capacity];

            for (int i = 0; i < capacity; i++)
                ValueArray[i] = default;
        }

        public AutoArray(IEnumerable<T> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var count = items.Count();
            var enumerator = items.GetEnumerator();

            var size = count;
            var resize = size > 10 ? size / 2 : size * 2;

            if (size < 1)
            {
                size = 100;
                resize = 50;
            }

            _free = size - count;
            _size = count;
            _last = -1;

            if (_free < 1)
            {
                size += resize;
                _free += resize;
            }

            ValueArray = new T[size];

            var index = 0;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is null)
                    throw new ArgumentNullException(nameof(enumerator.Current));

                ValueArray[index++] = enumerator.Current;
            }

            for (int i = index; i < size; i++)
                ValueArray[i] = default;
        }

        public T this[int index]
        {
            get => ValueArray[index];
            set => ValueArray[index] = value;
        }

        public int Add(T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var index = NextIndexOrResize(1);

            SetIndex(index, value);
            return index;
        }

        public bool Remove(T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var index = ValueArray.IndexOf(value);

            if (index < 0 || index >= ValueArray.Length)
                return false;

            SetIndex(index, default);
            return true;
        }
        
        public bool Contains(T value)
        {
            if (value is null)
                return false;

            return ValueArray.Contains(value);
        }

        public bool Any(Predicate<T> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                if (!predicate(ValueArray[i]))
                    continue;

                return true;
            }

            return false;
        }

        public int RemoveMany(Predicate<T> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var count = 0;

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                if (!predicate(ValueArray[i]))
                    continue;

                SetIndex(i, default);
                count++;
            }

            return count;
        }

        public int RemoveMany(IEnumerable<T> values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            return RemoveMany(x => values.Contains(x));
        }

        public int FindIndex(T value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return ValueArray.FindIndex(x => x != null && x.Equals(value));
        }

        public void AddMany(IEnumerable<T> values)
            => AddMany(values, null);

        public void AddMany(IEnumerable<T> values, Predicate<T> predicate)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            var count = values.Count();

            if (count < 1)
                return;

            if (_free < count)
                Resize(count);

            var index = NextIndex();
            var enumerator = values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is null)
                    continue;

                if (predicate != null && !predicate(enumerator.Current))
                    continue;

                SetIndex(index, enumerator.Current);

                index = NextIndex();
            }
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                action(ValueArray[i]);
            }
        }

        public void ForEach(Action<T> action, Predicate<T> predicate)
        {
            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                var item = ValueArray[i];

                if (!predicate(item))
                    continue;
                    
                action(item);
            }
        }

        public void For(Action<int, T> action)
        {
            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                action(i, ValueArray[i]);
            }
        }

        public void For(Action<int, T> action, Func<int, T, bool> predicate)
        {
            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                var item = ValueArray[i];

                if (!predicate(i, item))
                    continue;

                action(i, item);
            }
        }

        public bool TryGet(Predicate<T> predicate, out T value)
        {
            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                var item = ValueArray[i];

                if (!predicate(item))
                    continue;

                value = item;
                return true;
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < ValueArray.Length; i++)
                ValueArray[i] = default;

            _free = ValueArray.Length;

            _size = 0;
            _last = -1;
        }

        public IEnumerable<T> Where(Predicate<T> predicate)
            => ValueArray.Where(x => x != null && predicate(x));

        public List<T> WhereList(Predicate<T> predicate)
            => ValueArray.Where(x => x != null && predicate(x)).ToList();

        public HashSet<T> WhereHashSet(Predicate<T> predicate)
            => ValueArray.Where(x => x != null && predicate(x)).ToHashSet();

        public T[] WhereArray(Predicate<T> predicate)
            => ValueArray.Where(x => x != null && predicate(x)).ToArray();

        public List<T> ToList()
        {
            var list = new List<T>(_size);

            ToList(list);
            return list;
        }

        public List<T> ToPooledList()
        {
            var list = ListPool<T>.Shared.Rent(_size);

            ToList(list);
            return list;
        }

        public HashSet<T> ToHashSet()
        {
            var set = new HashSet<T>(_size);

            ToCollection(set);
            return set;
        }

        public T[] ToArray()
        {
            var array = new T[_size];
            var index = 0;

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                array[index++] = ValueArray[i];
            }

            return array;
        }

        public void ToList(List<T> list)
        {
            list.Clear();

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                list.Add(ValueArray[i]);
            }
        }

        public void ToCollection(ICollection<T> collection)
        {
            collection.Clear();

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] is null)
                    continue;

                collection.Add(ValueArray[i]);
            }
        }

        private int NextIndexOrResize(int? resizeAdd = null)
        {
            if (_free < 0)
                Resize(resizeAdd);

            return NextIndex();
        }

        private int NextIndex()
        {
            if (_last != -1 && ValueArray[_last] is null)
                return _last;

            for (int i = 0; i < ValueArray.Length; i++)
            {
                if (ValueArray[i] != null)
                    continue;

                return i;
            }

            return -1;
        }

        private void SetIndex(int index, T value)
        {
            if (value != null)
            {
                ValueArray[index] = value;

                _size++;
                _free--;

                if (index == _last)
                    _last = -1;
            }
            else
            {
                ValueArray[index] = default;

                _size--;
                _free++;

                _last = index;
            }
        }

        private void Resize(int? addedSize = null)
        {
            var totalCount = ResizeCount;

            if (addedSize.HasValue && addedSize.Value > 0)
                totalCount += addedSize.Value;

            if (totalCount < 1)
                throw new Exception($"This array cannot be resized.");

            _free = 0;
            _size = 0;

            var newValueArray = new T[ValueArray.Length + totalCount];

            for (int i = 0; i < newValueArray.Length; i++)
            {
                if (i < ValueArray.Length)
                {
                    newValueArray[i] = ValueArray[i];

                    if (ValueArray[i] != null)
                        _size++;
                    else
                        _free++;
                }
                else
                {
                    newValueArray[i] = default;

                    _free++;
                }
            }

            ValueArray = newValueArray;
        }
    }
}