using LabExtended.API;
using LabExtended.Utilities.Generation;

using NorthwoodLib.Pools;

namespace LabExtended.Utilities
{
    public static class WeightUtils
    {
        private static readonly bool[] _boolArray = new bool[] { true, false };
        
        public static bool GetBool(float trueChance = 50f, float falseChance = 50f, bool validateWeight = false)
            => GetRandomWeighted(_boolArray, value => value ? trueChance : falseChance, validateWeight);

        public static bool GetBool(float trueChance = 50f, bool validateWeight = false)
            => GetRandomWeighted(_boolArray, value => value ? trueChance : 100 - trueChance, validateWeight);

        public static KeyValuePair<TKey, TValue> GetRandomPair<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
            => GetRandomWeighted(dict, pair => weightPicker(pair.Key, pair.Value), validateWeight);

        public static TKey GetRandomKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
            => GetRandomPair(dict, weightPicker, validateWeight).Key;

        public static TValue GetRandomValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
            => GetRandomPair(dict, weightPicker, validateWeight).Value;

        public static T[] GetRandomWeightedArray<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var count = items.Count();

            if (count < minCount) throw new Exception($"Not enough items in collection ({count} / {minCount}).");

            var array = new T[minCount];
            var total = items.Sum(x => weightPicker(x));

            if (total != 100f && validateWeight) throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

            var list = ListPool<T>.Shared.Rent(items);
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                while (!allowDuplicates && selected.Contains(index) && ExServer.IsRunning)
                    index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                array[i] = list[index];
            }

            ListPool<int>.Shared.Return(selected);
            ListPool<T>.Shared.Return(list);

            return array;
        }

        public static List<T> GetRandomWeightedList<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var count = items.Count();

            if (count < minCount) throw new Exception($"Not enough items in collection ({count} / {minCount}).");

            var chosen = new List<T>(minCount);
            var total = items.Sum(x => weightPicker(x));

            if (total != 100f && validateWeight) throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

            var list = ListPool<T>.Shared.Rent(items);
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                while (!allowDuplicates && selected.Contains(index) && ExServer.IsRunning)
                    index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                chosen.Add(list[index]);
            }

            ListPool<int>.Shared.Return(selected);
            ListPool<T>.Shared.Return(list);

            return chosen;
        }

        public static HashSet<T> GetRandomWeightedHashSet<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var count = items.Count();

            if (count < minCount) throw new Exception($"Not enough items in collection ({count} / {minCount}).");

            var chosen = new HashSet<T>(minCount);
            var total = items.Sum(x => weightPicker(x));

            if (total != 100f && validateWeight) throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

            var list = ListPool<T>.Shared.Rent(items);
            var selected = ListPool<int>.Shared.Rent();

            for (int i = 0; i < minCount; i++)
            {
                var index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                while (!allowDuplicates && selected.Contains(index) && ExServer.IsRunning)
                    index = GetRandomIndex(total, count, x => weightPicker(list[x]));

                chosen.Add(list[index]);
            }

            ListPool<int>.Shared.Return(selected);
            ListPool<T>.Shared.Return(list);

            return chosen;
        }

        public static T GetRandomWeighted<T>(this IEnumerable<T> items, Func<T, float> weightPicker, bool validateWeight = false)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var list = ListPool<T>.Shared.Rent(items);

            if (list.Count < 0)
            {
                ListPool<T>.Shared.Return(list);
                throw new ArgumentException($"Cannot pick from an empty list.");
            }

            if (list.Count == 1)
            {
                var first = list[0];

                ListPool<T>.Shared.Return(list);
                return first;
            }

            var total = list.Sum(val => weightPicker(val));

            if (total != 100f && validateWeight)
            {
                ListPool<T>.Shared.Return(list);
                throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");
            }

            var item = list[GetRandomIndex(total, list.Count, index => weightPicker(list[index]))];

            ListPool<T>.Shared.Return(list);
            return item;
        }

        public static int GetRandomIndex(float total, int size, Func<int, float> picker)
        {
            var rnd = RandomGen.Instance.GetFloat(0f, total);
            var sum = 0f;

            for (int i = 0; i < size; i++)
            {
                var weight = picker(i);

                for (float x = sum; x < weight + sum; x++)
                {
                    if (x >= rnd)
                        return i;
                }

                sum += weight;
            }

            return 0;
        }
    }
}