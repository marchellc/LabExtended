﻿using LabExtended.Utilities;

using NorthwoodLib.Pools;

using System.Collections;

namespace LabExtended.Extensions
{
    public static class CollectionExtensions
    {
        #region Random Selection Extensions
        public static T GetRandomItem<T>(this IEnumerable<T> items, Predicate<T> predicate = null)
        {
            var validItems = predicate != null ? items.Where(x => predicate(x)) : items;
            var count = validItems.Count();

            if (count == 0)
                throw new Exception($"Cannot select item in an empty collection");

            if (count < 2)
                return items.First();

            return validItems.ElementAt(RandomGen.Instance.GetInt32(0, count - 1));
        }

        public static T[] GetRandomArray<T>(this IEnumerable<T> items, int minCount, Predicate<T> predicate = null)
        {
            var validItems = predicate != null ? items.Where(x => predicate(x)) : items;
            var count = validItems.Count();

            if (count < minCount)
                throw new Exception($"Not enough items to select ({count} / {minCount})");

            var array = new T[minCount];
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = RandomGen.Instance.GetInt32(0, count - 1);

                while (selected.Contains(index))
                    index = RandomGen.Instance.GetInt32(0, count - 1);

                selected.Add(index);
                array[i] = items.ElementAt(index);
            }

            ListPool<int>.Shared.Return(selected);
            return array;
        }

        public static List<T> GetRandomList<T>(this IEnumerable<T> items, int minCount, Predicate<T> predicate = null)
        {
            var validItems = predicate != null ? items.Where(x => predicate(x)) : items;
            var count = validItems.Count();

            if (count < minCount)
                throw new Exception($"Not enough items to select ({count} / {minCount})");

            var list = new List<T>(minCount);
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = RandomGen.Instance.GetInt32(0, count - 1);

                while (selected.Contains(index))
                    index = RandomGen.Instance.GetInt32(0, count - 1);

                selected.Add(index);
                list.Add(items.ElementAt(index));
            }

            ListPool<int>.Shared.Return(selected);
            return list;
        }

        public static HashSet<T> GetRandomHashSet<T>(this IEnumerable<T> items, int minCount, Predicate<T> predicate = null)
        {
            var validItems = predicate != null ? items.Where(x => predicate(x)) : items;
            var count = validItems.Count();

            if (count < minCount)
                throw new Exception($"Not enough items to select ({count} / {minCount})");

            var set = new HashSet<T>(minCount);
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = RandomGen.Instance.GetInt32(0, count - 1);

                while (selected.Contains(index))
                    index = RandomGen.Instance.GetInt32(0, count - 1);

                selected.Add(index);
                set.Add(items.ElementAt(index));
            }

            ListPool<int>.Shared.Return(selected);
            return set;
        }
        #endregion

        #region Array Extensions
        public static void SetIndex<T>(this ArraySegment<T> segment, int index, T value)
            => segment.Array[index] = value;

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
        #endregion

        #region Collection Extensions
        public static T RemoveAndTake<T>(this IList<T> list, int index)
        {
            var value = list[index];

            list.RemoveAt(index);
            return value;
        }

        public static List<T> TakeWhere<T>(this ICollection<T> objects, int count, Predicate<T> predicate)
        {
            if (objects.Count(o => predicate(o)) < count)
                return null;

            var list = new List<T>(count);
            var added = 0;

            while (added != count)
            {
                var item = objects.First(o => predicate(o));

                objects.Remove(item);
                list.Add(item);

                added++;
            }

            return list;
        }
        #endregion

        #region Enumerable Extensions
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
        #endregion

        #region Dictionary Extensions
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
        #endregion

        #region Queue Extensions
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
        #endregion
    }
}
