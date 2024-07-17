using LabExtended.Extensions;

namespace LabExtended.API.Pooling
{
    public static class CollectionPool<TCollection, TElement> where TCollection : IList<TElement>
    {
        private static readonly List<TCollection> _pool = new List<TCollection>();

        public static int Size => _pool.Count;

        public static TCollection Create()
            => Activator.CreateInstance<TCollection>();

        public static TCollection Rent(Action<TCollection> setup = null)
        {
            var value = default(TCollection);

            if (_pool.Count < 1)
                value = Create();
            else
                value = _pool.RemoveAndTake(0);

            setup.InvokeSafe(value);
            return value;
        }

        public static void Return(TCollection value, Action<TCollection> cleanup = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            cleanup.InvokeSafe(value);
            value.Clear();

            _pool.Add(value);
        }

        public static TElement[] ToArrayReturn(TCollection value, Action<TCollection> cleanup)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var array = value.ToArray();

            cleanup.InvokeSafe(value);
            value.Clear();

            _pool.Add(value);
            return array;
        }
    }
}