using LabExtended.Extensions;

using System.Collections.Concurrent;

namespace LabExtended.API.Pooling
{
    public static class ObjectPool<T>
    {
        private static volatile ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();

        public static int Size => _pool.Count;

        public static T Create()
            => Activator.CreateInstance<T>();

        public static void PreConstruct(int size, Func<T> create = null)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            for (int i = 0; i < size; i++)
            {
                if (create != null)
                    _pool.Enqueue(create());
                else
                    _pool.Enqueue(Create());
            }
        }

        public static T Rent(Action<T> setup = null, Func<T> create = null)
        {
            if (!_pool.TryDequeue(out var item))
            {
                if (create != null)
                    item = create();
                else
                    item = Create();
            }

            setup.InvokeSafe(item);
            return item;
        }

        public static void Return(T value, Action<T> cleanup = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            cleanup.InvokeSafe(value);

            _pool.Enqueue(value);
        }
    }
}