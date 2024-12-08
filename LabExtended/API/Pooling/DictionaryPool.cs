using System.Collections.Concurrent;

namespace LabExtended.API.Pooling
{
    public static class DictionaryPool<TKey, TElement>
    {
        private static readonly ConcurrentQueue<Dictionary<TKey, TElement>> _pool = new ConcurrentQueue<Dictionary<TKey, TElement>>();

        public static Dictionary<TKey, TElement> Rent()
        {
            if (!_pool.TryDequeue(out var dict))
                return new Dictionary<TKey, TElement>();

            return dict;
        }

        public static void Return(Dictionary<TKey, TElement> dict)
        {
            if (dict is null)
                throw new ArgumentNullException(nameof(dict));

            dict.Clear();

            _pool.Enqueue(dict);
        }
    }
}