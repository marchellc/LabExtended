using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Utilities.Generation
{
    public class UniqueGenerator<T>
    {
        private volatile Func<T> _generator;
        private volatile HashSet<T> _cache = new();

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

            while (_cache.Contains(value) && ExServer.IsRunning)
                value = _generator();

            _cache.Add(value);
            return value;
        }

        public void Free(T value)
            => _cache.Remove(value);

        public void FreeAll()
            => _cache.Clear();

        public void FreeAll(Func<T, bool> predicate)
            => _cache.RemoveWhere(x => predicate(x));

        public void Occupy(T value)
            => _cache.Add(value);

        public void OccupyMany(IEnumerable<T> values)
            => values?.ForEach(x => _cache.Add(x));

        public bool IsOccupied(T value)
            => _cache.Contains(value);
    }
}