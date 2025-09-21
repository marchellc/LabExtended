using LabExtended.API;

using NorthwoodLib.Pools;

using System.Collections;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting collections.
/// </summary>
public static class CollectionExtensions
{
    #region Random Selection Extensions
    /// <summary>
    /// Gets a random item.
    /// </summary>
    public static T GetRandomItem<T>(this IEnumerable<T> items, Predicate<T>? predicate = null)
    {
        var validItems = predicate != null ? items.Where(x => predicate(x)) : items;
        var count = validItems.Count();

        if (count == 0)
            throw new Exception($"Cannot select item in an empty collection");

        if (count < 2)
            return items.First();

        return validItems.ElementAt(UnityEngine.Random.Range(0, count));
    }

    /// <summary>
    /// Gets a random array of items.
    /// </summary>
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
            var index = UnityEngine.Random.Range(0, count);

            while (selected.Contains(index))
                index = UnityEngine.Random.Range(0, count);

            selected.Add(index);
            array[i] = items.ElementAt(index);
        }

        ListPool<int>.Shared.Return(selected);
        return array;
    }

    /// <summary>
    /// Gets a random list of items.
    /// </summary>
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
            var index = UnityEngine.Random.Range(0, count);

            while (selected.Contains(index))
                index = UnityEngine.Random.Range(0, count);

            selected.Add(index);
            list.Add(items.ElementAt(index));
        }

        ListPool<int>.Shared.Return(selected);
        return list;
    }

    /// <summary>
    /// Gets a random hashset of items.
    /// </summary>
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
            var index = UnityEngine.Random.Range(0, count);

            while (selected.Contains(index))
                index = UnityEngine.Random.Range(0, count);

            selected.Add(index);
            set.Add(items.ElementAt(index));
        }

        ListPool<int>.Shared.Return(selected);
        return set;
    }
    #endregion

    #region Array Extensions
    /// <summary>
    /// Sets the element in an array segment.
    /// </summary>
    public static void SetIndex<T>(this ArraySegment<T> segment, int index, T value)
        => segment.Array[index] = value;

    /// <summary>
    /// Finds the index of a predicate.
    /// </summary>
    public static int FindIndex<T>(this T[] array, Predicate<T> predicate)
        => Array.FindIndex(array, predicate);

    /// <summary>
    /// Casts a collection to an array.
    /// </summary>
    public static T[] CastArray<T>(this IEnumerable values)
        => values.Cast<T>().ToArray();

    /// <summary>
    /// Attempts to peek a specific index in an array.
    /// </summary>
    public static bool TryPeekIndex<T>(this T[] array, int index, out T value)
    {
        if (index < 0 || index >= array.Length)
        {
            value = default!;
            return false;
        }

        value = array[index];
        return true;
    }
    #endregion

    #region Collection Extensions

    /// <summary>
    /// Adds an item if it isn't in the list.
    /// </summary>
    public static bool AddUnique<T>(this IList<T> list, T item)
    {
        if (list.Contains(item))
            return false;
        
        list.Add(item);
        return true;
    }

    /// <summary>
    /// Removes an item if it's the only one in the list.
    /// </summary>
    public static bool RemoveIfUnique<T>(this IList<T> list, T item)
    {
        if (list.Count(x => x.Equals(item)) == 1)
            return list.Remove(item);

        return false;
    }
    
    /// <summary>
    /// Removes an item at a specific index and returns it.
    /// </summary>
    public static T RemoveAndTake<T>(this IList<T> list, int index)
    {
        var value = list[index];

        list.RemoveAt(index);
        return value;
    }

    /// <summary>
    /// Takes all elements matching a predicate.
    /// </summary>
    public static List<T> TakeWhere<T>(this ICollection<T> objects, int count, Predicate<T> predicate)
    {
        if (objects.Count(o => predicate(o)) < count)
            return null!;

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

    /// <summary>
    /// Adds all elements matching a predicate.
    /// </summary>
    public static void AddRangeWhere<T>(this ICollection<T> collection, IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source is List<T> sourceList)
        {
            sourceList.ForEach(s =>
            {
                if (!predicate(s))
                    return;
                
                collection.Add(s);
            });
        }
        else
        {
            foreach (var item in source)
            {
                if (!predicate(item))
                    continue;
                
                collection.Add(item);
            }
        }
    }

    /// <summary>
    /// ToList() with a pooled list instance.
    /// </summary>
    public static List<T> ToPooledList<T>(this IEnumerable<T> objects)
        => ListPool<T>.Shared.Rent(objects);

    /// <summary>
    /// Returns a pool list to the shared pool.
    /// </summary>
    public static void ReturnToPool<T>(this List<T> pooledList)
        => ListPool<T>.Shared.Return(pooledList);
    #endregion

    #region Enumerable Extensions
    /// <summary>
    /// Gets the amount of elements in an enumerable using their native properties (if defined), otherwise using LINQ.
    /// </summary>
    public static int GetCountFast(this IEnumerable objects)
    {
        if (objects is null)
            return 0;

        if (objects is ICollection collection)
            return collection.Count;

        if (objects is Array array)
            return array.Length;

        var count = 0;
        var enumerator = objects.GetEnumerator();

        while (enumerator.MoveNext() && ExServer.IsRunning)
            count++;

        if (enumerator is IDisposable disposable)
            disposable.Dispose();

        return count;
    }

    /// <summary>
    /// Returns a collection of elements from the input sequence that are of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>Elements in the input sequence that are not of type <typeparamref name="T"/> are excluded
    /// from the result. The returned sequence preserves the order of the original sequence.</remarks>
    /// <typeparam name="T">The type to filter the elements of the sequence on.</typeparam>
    /// <param name="objects">The sequence of objects to filter.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements from the input sequence of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> Where<T>(this IEnumerable<object> objects)
        => objects.Where(obj => obj is T).Select(obj => (T)obj);

    /// <summary>
    /// Filters the elements of an object sequence, returning only those of type T that satisfy a specified condition.
    /// </summary>
    /// <remarks>Elements in the input sequence that are not of type T are ignored. The method uses deferred
    /// execution; the filtering is performed when the returned sequence is enumerated.</remarks>
    /// <typeparam name="T">The type to filter the elements of the sequence on.</typeparam>
    /// <param name="objects">The sequence of objects to filter.</param>
    /// <param name="predicate">A function to test each element of type T for a condition. Only elements for which the predicate returns <see
    /// langword="true"/> are included in the result.</param>
    /// <returns>An IEnumerable{T} that contains elements of type T from the input sequence that satisfy the condition specified by the predicate.</returns>
    public static IEnumerable<T> Where<T>(this IEnumerable<object> objects, Func<T, bool> predicate)
        => objects.Where(obj => obj is T && predicate((T)obj)).Select(obj => (T)obj);

    /// <summary>
    /// Performs the specified action on each element of the enumerable collection.
    /// </summary>
    /// <remarks>This method is typically used to execute an operation for each item in a collection without
    /// creating a new collection. The order in which the action is performed corresponds to the order of the elements
    /// in the enumerable. If either <paramref name="values"/> or <paramref name="action"/> is null, an <see
    /// cref="ArgumentNullException"/> is thrown.</remarks>
    /// <typeparam name="T">The type of the elements in the enumerable collection.</typeparam>
    /// <param name="values">The enumerable collection whose elements the action will be performed on. Cannot be null.</param>
    /// <param name="action">The action to perform on each element of the collection. Cannot be null.</param>
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        foreach (var value in values)
            action(value);
    }

    /// <summary>
    /// Performs the specified action on each element of the sequence that satisfies the given predicate.
    /// </summary>
    /// <remarks>If the sequence is empty or no elements satisfy the predicate, the action is not performed.
    /// This method enumerates the sequence once and does not modify the original collection.</remarks>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="values">The sequence of elements to iterate over. Cannot be null.</param>
    /// <param name="predicate">A function to test each element for a condition. The action is performed only on elements for which this
    /// function returns <see langword="true"/>. Cannot be null.</param>
    /// <param name="action">The action to perform on each element that satisfies the predicate. Cannot be null.</param>
    public static void ForEach<T>(this IEnumerable<T> values, Func<T, bool> predicate, Action<T> action)
    {
        foreach (var value in values)
        {
            if (!predicate(value))
                continue;

            action(value);
        }
    }

    /// <summary>
    /// Attempts to find the first element in the sequence that satisfies the specified predicate.
    /// </summary>
    /// <remarks>Null elements in the sequence are ignored. If no matching element is found, the out parameter
    /// is set to the default value for the type.</remarks>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="objects">The sequence of elements to search.</param>
    /// <param name="predicate">A function to test each element for a condition. Cannot be null.</param>
    /// <param name="result">When this method returns true, contains the first element that matches the predicate, if found; otherwise, the
    /// default value for the type.</param>
    /// <returns>true if an element that satisfies the predicate is found; otherwise, false.</returns>
    public static bool TryGetFirst<T>(this IEnumerable<T> objects, Func<T, bool> predicate, out T result)
    {
        foreach (var obj in objects)
        {
            if (obj is null || obj is not T cast || !predicate(cast))
                continue;

            result = cast;
            return true;
        }

        result = default!;
        return false;
    }

    /// <summary>
    /// Attempts to find the first element of type T in the sequence that matches the specified predicate.
    /// </summary>
    /// <remarks>Elements in the sequence that are null or not of type T are ignored. The search stops at the
    /// first matching element.</remarks>
    /// <typeparam name="T">The type to search for within the sequence.</typeparam>
    /// <param name="objects">The sequence of objects to search.</param>
    /// <param name="predicate">A function to test each element of type T for a condition. Cannot be null.</param>
    /// <param name="result">When this method returns true, contains the first element of type T that matches the predicate, if found; otherwise,
    /// the default value for type T.</param>
    /// <returns>true if an element of type T that matches the predicate is found; otherwise, false.</returns>
    public static bool TryGetFirst<T>(this IEnumerable<object> objects, Func<T, bool> predicate, out T result)
    {
        foreach (var obj in objects)
        {
            if (obj is null || obj is not T cast || !predicate(cast))
                continue;

            result = cast;
            return true;
        }

        result = default!;
        return false;
    }

    /// <summary>
    /// Attempts to find the first element of a specified type in the sequence and returns a value that indicates
    /// whether the operation succeeded.
    /// </summary>
    /// <remarks>The search skips null values and elements that are not of the specified type. If no matching
    /// element is found, <paramref name="result"/> is set to the default value for the type.</remarks>
    /// <typeparam name="T">The type of element to search for in the sequence.</typeparam>
    /// <param name="objects">The sequence of objects to search.</param>
    /// <param name="result">When this method returns true, contains the first element of type <typeparamref name="T"/> if found; otherwise, the
    /// default value for the type.</param>
    /// <returns><see langword="true"/> if an element of type <typeparamref name="T"/> is found; otherwise, <see
    /// langword="false"/>.</returns>
    public static bool TryGetFirst<T>(this IEnumerable<object> objects, out T result)
    {
        foreach (var obj in objects)
        {
            if (obj is null || obj is not T cast)
                continue;

            result = cast;
            return true;
        }

        result = default!;
        return false;
    }
    #endregion

    #region Enumerator Extensions
    /// <summary>
    /// Executes the specified action for each element in the enumerator.
    /// </summary>
    /// <remarks>If disposeEnumerator is set to true and the enumerator implements IDisposable, the enumerator
    /// will be disposed after iteration. This method does not reset the enumerator before iteration; it starts from the
    /// enumerator's current position.</remarks>
    /// <param name="enumerator">The enumerator whose elements the action will be performed on. Cannot be null.</param>
    /// <param name="action">The action to perform on each element of the enumerator. The current element is passed as the parameter to the
    /// action. Cannot be null.</param>
    /// <param name="disposeEnumerator">true to dispose the enumerator after iteration if it implements IDisposable; otherwise, false. The default is
    /// true.</param>
    /// <exception cref="ArgumentNullException">Thrown if enumerator or action is null.</exception>
    public static void ForEach(this IEnumerator enumerator, Action<object> action, bool disposeEnumerator = true)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        while (enumerator.MoveNext())
            action(enumerator.Current);
        
        if (disposeEnumerator && enumerator is IDisposable disposable)
            disposable.Dispose();
    }
    
    /// <summary>
    /// Executes the specified action for each element in the enumerator.
    /// </summary>
    /// <remarks>If disposeEnumerator is set to true, the enumerator will be disposed after the action has
    /// been applied to all elements. If set to false, the enumerator remains usable after the method
    /// completes.</remarks>
    /// <typeparam name="T">The type of the elements in the enumerator.</typeparam>
    /// <param name="enumerator">The enumerator whose elements the action will be applied to. Cannot be null.</param>
    /// <param name="action">The action to perform on each element of the enumerator. Cannot be null.</param>
    /// <param name="disposeEnumerator">true to dispose the enumerator after iteration; otherwise, false. The default is true.</param>
    /// <exception cref="ArgumentNullException">Thrown if enumerator or action is null.</exception>
    public static void ForEach<T>(this IEnumerator<T> enumerator, Action<T> action, bool disposeEnumerator = true)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        while (enumerator.MoveNext())
            action(enumerator.Current);
        
        if (disposeEnumerator)
            enumerator.Dispose();
    }
    
    /// <summary>
    /// Executes the specified action for each element in the enumerator, providing the element's index and value.
    /// </summary>
    /// <remarks>If disposeEnumerator is set to true and the enumerator implements IDisposable, it will be
    /// disposed after iteration. The index parameter passed to the action starts at 0 and increments by 1 for each
    /// element.</remarks>
    /// <param name="enumerator">The enumerator to iterate over. Must not be null.</param>
    /// <param name="action">The action to perform on each element, receiving the zero-based index and the current element. Must not be null.</param>
    /// <param name="disposeEnumerator">true to dispose the enumerator after iteration if it implements IDisposable; otherwise, false. The default is
    /// true.</param>
    /// <exception cref="ArgumentNullException">Thrown if enumerator or action is null.</exception>
    public static void For(this IEnumerator enumerator, Action<int, object> action, bool disposeEnumerator = true)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var index = 0;
        
        while (enumerator.MoveNext())
            action(index++, enumerator.Current);
        
        if (disposeEnumerator && enumerator is IDisposable disposable)
            disposable.Dispose();
    }
    
    /// <summary>
    /// Executes the specified action for each element in the enumerator, providing the zero-based index and the element
    /// value.
    /// </summary>
    /// <remarks>If disposeEnumerator is set to true, the enumerator will be disposed after the action has
    /// been applied to all elements. If set to false, the enumerator remains usable after the method completes. The
    /// action receives the current index and element value for each iteration.</remarks>
    /// <typeparam name="T">The type of elements in the enumerator.</typeparam>
    /// <param name="enumerator">The enumerator whose elements the action will be applied to. Cannot be null.</param>
    /// <param name="action">The action to perform on each element, receiving the element's zero-based index and value. Cannot be null.</param>
    /// <param name="disposeEnumerator">true to dispose the enumerator after iteration; otherwise, false. The default is true.</param>
    /// <exception cref="ArgumentNullException">Thrown if enumerator or action is null.</exception>
    public static void For<T>(this IEnumerator<T> enumerator, Action<int, T> action, bool disposeEnumerator = true)
    {
        if (enumerator is null)
            throw new ArgumentNullException(nameof(enumerator));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var index = 0;
        
        while (enumerator.MoveNext())
            action(index++, enumerator.Current);
        
        if (disposeEnumerator)
            enumerator.Dispose();
    }
    #endregion

    #region Dictionary Extensions
    /// <summary>
    /// Attempts find a key by pair value.
    /// </summary>
    /// <typeparam name="TKey">The type of the key element.</typeparam>
    /// <typeparam name="TValue">The type of the value element.</typeparam>
    /// <param name="dictionary">The source dictionary.</param>
    /// <param name="key">The key to get the value of.</param>
    /// <param name="defaultValue">The value to return if the key is not present.</param>
    /// <returns>The value of the pair with the provided key -or- <paramref name="defaultValue"/> if the key is not defined in the dictionary</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));
        
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (!dictionary.TryGetValue(key, out var value))
            return defaultValue;

        return value;
    }

    /// <summary>
    /// Attempts to find a key for a pair's value.
    /// </summary>
    /// <typeparam name="TKey">The type of the key element.</typeparam>
    /// <typeparam name="TValue">The type of the value element.</typeparam>
    /// <param name="dict">The source dictionary.</param>
    /// <param name="value">The value of the pair.</param>
    /// <param name="key">The found key.</param>
    /// <returns>true if the key was found</returns>
    public static bool TryGetKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value, out TKey key)
    {
        foreach (var pair in dict)
        {
            if (pair.Value!.Equals(value))
            {
                key = pair.Key;
                return true;
            }
        }

        key = default!;
        return false;
    }

    /// <summary>
    /// Attempts find a key by pair value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value element.</typeparam>
    /// <param name="dictionary">The source dictionary.</param>
    /// <param name="key">The key to get the value of.</param>
    /// <param name="value">The value of the pair.</param>
    /// <returns>true if the pair with the provided key was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetValue<TValue>(this IDictionary dictionary, object key, out TValue? value)
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (!dictionary.Contains(key))
        {
            value = default;
            return false;
        }

        if (dictionary[key] is not TValue result)
        {
            value = default;
            return false;
        }

        value = result;
        return true;
    }

    /// <summary>
    /// Attempts find a key by pair value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value element.</typeparam>
    /// <param name="dictionary">The source dictionary.</param>
    /// <param name="key">The key to get the value of.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <returns>The value of the pair with the provided key.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TValue? GetValueOrDefault<TValue>(this IDictionary dictionary, object key, TValue? defaultValue = default)
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (!dictionary.Contains(key)
            || dictionary[key] is not TValue result)
            return defaultValue;

        return result;
    }

    /// <summary>
    /// Converts a collection of key-value pairs to a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key element.</typeparam>
    /// <typeparam name="TElement">The type of the dictionary value element.</typeparam>
    /// <param name="pairs">The collection of pairs to convert.</param>
    /// <returns>The converted dictionary.</returns>
    public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(this IEnumerable<KeyValuePair<TKey, TElement>> pairs)
    {
        var dict = new Dictionary<TKey, TElement>();

        foreach (var pair in pairs)
        {
            dict.Add(pair.Key, pair.Value);
        }

        return dict;
    }

    /// <summary>
    /// Adds the elements of the specified collection to the dictionary.
    /// </summary>
    /// <remarks>If a key in the collection already exists in the dictionary, an exception is thrown. The
    /// order in which elements are added corresponds to the order of the input collection.</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to which the key/value pairs will be added. Cannot be null.</param>
    /// <param name="pairs">The collection of key/value pairs to add to the dictionary. Cannot be null.</param>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        foreach (var pair in pairs)
        {
            dict.Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Adds a range of key/value pairs to the dictionary, using the specified keys and a selector function to generate
    /// values for each key.
    /// </summary>
    /// <remarks>Each key from the collection is added to the dictionary with a value produced by the selector
    /// function. If a key already exists in the dictionary, an exception is thrown. This method is an extension method
    /// for IDictionary{TKey, TValue}.</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to which the key/value pairs will be added.</param>
    /// <param name="keys">The collection of keys to add to the dictionary.</param>
    /// <param name="selector">A function that generates a value for each key.</param>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys, Func<TKey, TValue> selector)
    {
        foreach (var key in keys)
        {
            dict.Add(key, selector(key));
        }
    }

    /// <summary>
    /// Returns the zero-based index of the specified key in the dictionary, or -1 if the key is not found.
    /// </summary>
    /// <remarks>The order of elements is determined by the dictionary's enumeration, which may not correspond
    /// to insertion order unless the dictionary type preserves order (such as OrderedDictionary).</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search for the specified key. Cannot be null.</param>
    /// <param name="key">The key to locate in the dictionary. Cannot be null.</param>
    /// <returns>The zero-based index of the specified key if found; otherwise, -1.</returns>
    public static int FindKeyIndex<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
    {
        var index = 0;

        foreach (var pair in dict)
        {
            if (pair.Key!.Equals(key))
                return index;
            else
                index++;
        }

        return -1;
    }

    /// <summary>
    /// Searches for the first key in the dictionary that matches the specified predicate and returns its zero-based
    /// index.
    /// </summary>
    /// <remarks>The order of keys is determined by the dictionary's enumeration order, which may not be
    /// consistent across different dictionary implementations. This method does not guarantee thread safety if the
    /// dictionary is modified during enumeration.</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search. Cannot be null.</param>
    /// <param name="predicate">The predicate used to test each key. Cannot be null.</param>
    /// <returns>The zero-based index of the first key that matches the predicate; otherwise, -1 if no matching key is found.</returns>
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

    /// <summary>
    /// Replaces the element at the specified index in the dictionary with a new key-value pair.
    /// </summary>
    /// <remarks>The order of elements in the dictionary is determined by its current enumeration order. After
    /// replacement, the dictionary will contain the same number of elements, but the element at the specified index
    /// will be replaced by newPair. If newPair.Key already exists in the dictionary, an exception may be
    /// thrown.</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary whose element is to be replaced. Must not be null.</param>
    /// <param name="targetIndex">The zero-based index of the element to replace. Must be greater than or equal to 0 and less than the number of
    /// elements in the dictionary.</param>
    /// <param name="newPair">The new key-value pair to insert at the specified index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when targetIndex is less than 0 or greater than or equal to the number of elements in the dictionary.</exception>
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

    /// <summary>
    /// Attempts to find the first key/value pair in the dictionary that matches the specified predicate.
    /// </summary>
    /// <remarks>Enumeration is performed in the order defined by the dictionary's enumerator. If no element
    /// matches the predicate, the out parameter is set to its default value.</remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to search for a matching key/value pair.</param>
    /// <param name="predicate">The predicate used to test each key/value pair. The method returns the first pair for which this predicate
    /// returns <see langword="true"/>.</param>
    /// <param name="pair">When this method returns, contains the first key/value pair that matches the predicate, if found; otherwise, the
    /// default value for <see cref="KeyValuePair{TKey, TValue}"/>.</param>
    /// <returns><see langword="true"/> if a matching key/value pair is found; otherwise, <see langword="false"/>.</returns>
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
    /// <summary>
    /// Removes the first occurrence of a specific value from the queue.
    /// </summary>
    /// <remarks>If the specified value is not found in the queue, the queue remains unchanged. The order of
    /// the remaining elements is preserved.</remarks>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    /// <param name="queue">The queue from which to remove the value. Cannot be null.</param>
    /// <param name="value">The value to remove from the queue. The first occurrence of this value will be removed if found.</param>
    public static void Remove<T>(this Queue<T> queue, T value)
    {
        var values = queue.ToList();

        values.Remove(value);

        queue.EnqueueMany(values);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the queue.
    /// </summary>
    /// <remarks>The elements are enqueued in the order they are returned by the <paramref name="values"/>
    /// enumerable. This method does not clear the queue before adding new elements.</remarks>
    /// <typeparam name="T">The type of elements in the queue and the collection to enqueue.</typeparam>
    /// <param name="queue">The queue to which the elements will be added. Cannot be null.</param>
    /// <param name="values">The collection of elements to add to the queue. Cannot be null.</param>
    public static void EnqueueMany<T>(this Queue<T> queue, IEnumerable<T> values)
    {
        foreach (var item in values)
        {
            queue.Enqueue(item);
        }
    }
    #endregion
}
