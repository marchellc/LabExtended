using LabExtended.API.Collections.Locked;

namespace LabExtended.Utilities.Generation
{
    public class UniqueGenerator<T>
    {
        private volatile Func<T> _generator;
        private volatile LockedHashSet<T> _cache = new LockedHashSet<T>();

        public ICollection<T> Cache => _cache;

        public void SetGenerator(Func<T> generator)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));

            _generator = generator;
        }

        public T Next()
        {
            var value = _generator();

            while (_cache.Contains(value))
                value = _generator();

            _cache.Add(value);
            return value;
        }

        public void Free(T value)
            => _cache.Remove(value);

        public void FreeAll()
            => _cache.RemoveAll(value => true);

        public void FreeAll(Func<T, bool> predicate)
            => _cache.RemoveAll(predicate);

        public void Occupy(T value)
            => _cache.Add(value);

        public void OccupyMany(IEnumerable<T> values)
            => _cache.AddRange(values);

        public bool IsOccupied(T value)
            => _cache.Contains(value);
    }
}