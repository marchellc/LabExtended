using System.Collections;

namespace LabExtended.Extensions
{
    public static class CollectionExtensions
    {
        public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
            => Array.FindIndex(array, predicate);

        public static T[] CastArray<T>(this IEnumerable values)
            => values.Cast<T>().ToArray();

        public static bool TryPeekIndex<T>(this T[] array, int index, out T value)
        {
            if (index < 0 || index >= array.Length)
            {
                value = default;
                return false;
            }

            value = array[index];
            return true;
        }

        public static T RemoveAndTake<T>(this IList<T> list, int index)
        {
            var value = list[index];

            list.RemoveAt(index);
            return value;
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<object> objects)
            => objects.Where(obj => obj is T).Select(obj => (T)obj);

        public static IEnumerable<T> Where<T>(this IEnumerable<object> objects, Func<T, bool> predicate)
            => objects.Where(obj => obj is T && predicate((T)obj)).Select(obj => (T)obj);

        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var value in values)
                action(value);
        }

        public static void ForEach<T>(this IEnumerable<T> values, Func<T, bool> predicate, Action<T> action)
        {
            foreach (var value in values)
            {
                if (!predicate(value))
                    continue;

                action(value);
            }
        }

        public static bool TryGetFirst<T>(this IEnumerable<T> objects, Func<T, bool> predicate, out T result)
        {
            foreach (var obj in objects)
            {
                if (obj is null || obj is not T cast || !predicate(cast))
                    continue;

                result = cast;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable<object> objects, Func<T, bool> predicate, out T result)
        {
            foreach (var obj in objects)
            {
                if (obj is null || obj is not T cast || !predicate(cast))
                    continue;

                result = cast;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable<object> objects, out T result)
        {
            foreach (var obj in objects)
            {
                if (obj is null || obj is not T cast)
                    continue;

                result = cast;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value, out TKey key)
        {
            foreach (var pair in dict)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    return true;
                }
            }

            key = default;
            return false;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(this IEnumerable<KeyValuePair<TKey, TElement>> pairs)
        {
            var dict = new Dictionary<TKey, TElement>();

            foreach (var pair in pairs)
                dict.Add(pair.Key, pair.Value);

            return dict;
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            foreach (var pair in pairs)
                dict.Add(pair.Key, pair.Value);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys, Func<TKey, TValue> selector)
        {
            foreach (var key in keys)
                dict.Add(key, selector(key));
        }

        public static int FindKeyIndex<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            var index = 0;

            foreach (var pair in dict)
            {
                if (pair.Key.Equals(key))
                    return index;
                else
                    index++;
            }

            return -1;
        }

        public static int FindKeyIndex<TKey, TValue>(this IDictionary<TKey, TValue> dict, Predicate<TKey> predicate)
        {
            var index = 0;

            foreach (var pair in dict)
            {
                if (predicate(pair.Key))
                    return index;
                else
                    index++;
            }

            return -1;
        }

        public static void SetKeyIndex<TKey, TValue>(this IDictionary<TKey, TValue> dict, int targetIndex, KeyValuePair<TKey, TValue> newPair)
        {
            if (targetIndex < 0 || targetIndex >= dict.Count)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            var copy = new Dictionary<TKey, TValue>(dict);
            var index = 0;

            dict.Clear();

            foreach (var pair in copy)
            {
                if (index == targetIndex)
                    dict.Add(pair.Key, pair.Value);
                else
                    dict.Add(newPair.Key, newPair.Value);

                index++;
            }
        }

        public static bool TryGetFirst<TKey, TValue>(this IDictionary<TKey, TValue> dict, Predicate<KeyValuePair<TKey, TValue>> predicate, out KeyValuePair<TKey, TValue> pair)
        {
            foreach (var item in dict)
            {
                if (!predicate(item))
                    continue;

                pair = item;
                return true;
            }

            pair = default;
            return false;
        }

        public static void Remove<T>(this Queue<T> queue, T value)
        {
            var values = queue.ToList();

            values.Remove(value);
            queue.EnqueueMany(values);
        }

        public static void EnqueueMany<T>(this Queue<T> queue, IEnumerable<T> values)
        {
            foreach (var item in values)
                queue.Enqueue(item);
        }
    }
}
