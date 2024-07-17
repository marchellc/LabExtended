using LabExtended.Extensions;

namespace LabExtended.API.Pooling
{
    public static class DictionaryPool<TKey, TElement>
    {
        private static readonly List<Dictionary<TKey, TElement>> _pool = new List<Dictionary<TKey, TElement>>();

        public static Dictionary<TKey, TElement> Rent()
        {
            if (_pool.Count < 1)
                return new Dictionary<TKey, TElement>();

            return _pool.RemoveAndTake(0);
        }

        public static void Return(Dictionary<TKey, TElement> dict)
        {
            dict.Clear();

            _pool.Add(dict);
        }
    }
}