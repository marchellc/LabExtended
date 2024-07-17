using LabExtended.Extensions;

namespace LabExtended.API.Pooling
{
    public static class ObjectPool<T>
    {
        private static readonly List<T> _pool = new List<T>();

        public static int Size => _pool.Count;

        public static T Create()
            => Activator.CreateInstance<T>();

        public static T Rent(Action<T> setup = null)
        {
            var value = default(T);

            if (_pool.Count < 1)
                value = Create();
            else
                value = _pool.RemoveAndTake(0);

            setup.InvokeSafe(value);
            return value;
        }

        public static void Return(T value, Action<T> cleanup = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            cleanup.InvokeSafe(value);

            _pool.Add(value);
        }
    }
}