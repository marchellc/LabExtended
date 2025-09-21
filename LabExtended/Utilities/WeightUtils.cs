using LabExtended.API;

using NorthwoodLib.Pools;

namespace LabExtended.Utilities;

/// <summary>
/// Used to help with selecting random items with weight.
/// </summary>
public static class WeightUtils
{
    private static readonly bool[] _boolArray = new bool[] { true, false };

    /// <summary>
    /// Selects a boolean.
    /// </summary>
    /// <param name="trueChance">The chance of the result being true.</param>
    /// <param name="falseChance">The chance of the result being false.</param>
    /// <param name="validateWeight">Whether or not to validate the provided weight.</param>
    /// <returns>The randomly picked result.</returns>
    public static bool GetBool(float trueChance = 50f, float falseChance = 50f, bool validateWeight = false)
        => GetRandomWeighted(_boolArray, value => value ? trueChance : falseChance, validateWeight);

    /// <summary>
    /// Selects a boolean.
    /// </summary>
    /// <param name="trueChance">The chance of the result being true.</param>
    /// <returns>The randomly picked result.</returns>
    public static bool GetBool(float trueChance = 50f)
        => GetRandomWeighted(_boolArray, value => value ? trueChance : 100 - trueChance, false);

    /// <summary>
    /// Selects a dictionary pair.
    /// </summary>
    /// <param name="dict">The source dictionary to select the pair from.</param>
    /// <param name="weightPicker">The delegate used to get weight of a dictionary pair.</param>
    /// <param name="validateWeight">Whether or not to validate the provided weight.</param>
    /// <returns>The randomly picked result.</returns>
    public static KeyValuePair<TKey, TValue> GetRandomPair<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
        => GetRandomWeighted(dict, pair => weightPicker(pair.Key, pair.Value), validateWeight);

    /// <summary>
    /// Selects a dictionary key.
    /// </summary>
    /// <param name="dict">The source dictionary to select the pair from.</param>
    /// <param name="weightPicker">The delegate used to get weight of a dictionary pair.</param>
    /// <param name="validateWeight">Whether or not to validate the provided weight.</param>
    /// <returns>The randomly picked result.</returns>
    public static TKey GetRandomKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
        => GetRandomPair(dict, weightPicker, validateWeight).Key;

    /// <summary>
    /// Selects a dictionary value.
    /// </summary>
    /// <param name="dict">The source dictionary to select the pair from.</param>
    /// <param name="weightPicker">The delegate used to get weight of a dictionary pair.</param>
    /// <param name="validateWeight">Whether or not to validate the provided weight.</param>
    /// <returns>The randomly picked result.</returns>
    public static TValue GetRandomValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, float> weightPicker, bool validateWeight = false)
        => GetRandomPair(dict, weightPicker, validateWeight).Value;

    /// <summary>
    /// Selects an array of items from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the array element.</typeparam>
    /// <param name="items">The source collection.</param>
    /// <param name="minCount">The minimum amount of items to select.</param>
    /// <param name="weightPicker">The delegate used to get weight of each collection item.</param>
    /// <param name="allowDuplicates">Whether or not to allow duplicate items in the array.</param>
    /// <param name="validateWeight">Whether or not to validate the total weight of the collection.</param>
    /// <returns>The array of randomly picked elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static T[] GetRandomWeightedArray<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
    {
        if (items is null) 
            throw new ArgumentNullException(nameof(items));

        var count = items.Count();

        if (count < minCount)
            throw new Exception($"Not enough items in collection ({count} / {minCount}).");

        var array = new T[minCount];
        var total = items.Sum(x => weightPicker(x));

        if (total != 100f && validateWeight)
            throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

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

    /// <summary>
    /// Selects a list of items from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the list element.</typeparam>
    /// <param name="items">The source collection.</param>
    /// <param name="minCount">The minimum amount of items to select.</param>
    /// <param name="weightPicker">The delegate used to get weight of each collection item.</param>
    /// <param name="allowDuplicates">Whether or not to allow duplicate items in the list.</param>
    /// <param name="validateWeight">Whether or not to validate the total weight of the collection.</param>
    /// <returns>The list of randomly picked elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static List<T> GetRandomWeightedList<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
    {
        if (items is null) 
            throw new ArgumentNullException(nameof(items));

        var count = items.Count();

        if (count < minCount) 
            throw new Exception($"Not enough items in collection ({count} / {minCount}).");

        var chosen = new List<T>(minCount);
        var total = items.Sum(x => weightPicker(x));

        if (total != 100f && validateWeight) 
            throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

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

    /// <summary>
    /// Selects a hashset of items from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the hashset element.</typeparam>
    /// <param name="items">The source collection.</param>
    /// <param name="minCount">The minimum amount of items to select.</param>
    /// <param name="weightPicker">The delegate used to get weight of each collection item.</param>
    /// <param name="allowDuplicates">Whether or not to allow duplicate items in the hashset.</param>
    /// <param name="validateWeight">Whether or not to validate the total weight of the collection.</param>
    /// <returns>The hashset of randomly picked elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static HashSet<T> GetRandomWeightedHashSet<T>(this IEnumerable<T> items, int minCount, Func<T, float> weightPicker, bool allowDuplicates = false, bool validateWeight = false)
    {
        if (items is null) 
            throw new ArgumentNullException(nameof(items));

        var count = items.Count();

        if (count < minCount)
            throw new Exception($"Not enough items in collection ({count} / {minCount}).");

        var chosen = new HashSet<T>(minCount);
        var total = items.Sum(x => weightPicker(x));

        if (total != 100f && validateWeight)
            throw new InvalidOperationException($"Cannot pick from list; it's chance sum is not equal to a hundred ({total}).");

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

    /// <summary>
    /// Selects a random item from a collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection element.</typeparam>
    /// <param name="items">The source collection.</param>
    /// <param name="weightPicker">The delegate used to get the weight of an item.</param>
    /// <param name="validateWeight">Whether or not to validate the total weight of the collection.</param>
    /// <returns>The randomly selected item.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static T GetRandomWeighted<T>(this IEnumerable<T> items, Func<T, float> weightPicker, bool validateWeight = false)
    {
        if (items is null) 
            throw new ArgumentNullException(nameof(items));

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

    /// <summary>
    /// Selects a random index.
    /// </summary>
    /// <param name="total">The total weight of all the items in the source collection.</param>
    /// <param name="size">The amount of items in the source collection.</param>
    /// <param name="picker">The delegate used to select weight of specific collection items, key being the index of the item.</param>
    /// <returns>The selected index.</returns>
    public static int GetRandomIndex(float total, int size, Func<int, float> picker)
    {
        var rnd = UnityEngine.Random.Range(0f, total);
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