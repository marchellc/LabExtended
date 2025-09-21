using System.Collections;
using System.Collections.ObjectModel;

namespace LabExtended.API.Collections.Unsafe;

/// <summary>
/// Represents a high-performance, array-backed list of elements that provides fast, low-level access and manipulation
/// without built-in thread safety or bounds checking. Intended for scenarios where maximum performance is required and
/// the caller is responsible for ensuring correct usage.
/// </summary>
/// <remarks>UnsafeList{T} is similar to List{T} but omits certain safety checks and thread synchronization to
/// maximize performance. It is not thread-safe and does not perform bounds checking on all operations; callers must
/// ensure that indices and capacities are valid. This type is suitable for advanced scenarios where the overhead of
/// additional safety is undesirable and the caller can guarantee correct usage. Modifying the collection while
/// enumerating it is not supported and may result in undefined behavior.</remarks>
/// <typeparam name="T">The type of elements stored in the list.</typeparam>
public class UnsafeList<T> : IList<T>, IList, IReadOnlyList<T>
{
    /// <summary>
    /// Gets the maximum possible size of an array.
    /// </summary>
    public const int MaxArrayLength = 0X7FFFFFC7;

    private static volatile T[] emptyArray = new T[0];

    internal volatile T[] array;
    internal volatile int size;

    /// <see cref="IList.IsFixedSize"/>
    bool IList.IsFixedSize => false;

    /// <see cref="ICollection{T}.IsReadOnly"/>
    bool ICollection<T>.IsReadOnly => false;

    /// <see cref="IList.IsReadOnly"/>
    bool IList.IsReadOnly => false;

    /// <see cref="ICollection.IsSynchronized"/>
    bool ICollection.IsSynchronized => false;

    /// <see cref="ICollection.SyncRoot"/>
    object ICollection.SyncRoot => this;

    /// <summary>
    /// Gets the amount of items in the collection.
    /// </summary>
    public int Count => size;

    /// <summary>
    /// Gets or sets the capacity of the collection.
    /// </summary>
    public int Capacity
    {
        get => array.Length;
        set
        {
            if (value < size)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value != array.Length)
            {
                if (value > 0)
                {
                    var newArray = new T[value];

                    if (size > 0)
                        Array.Copy(array, newArray, size);

                    array = newArray;
                }
                else
                {
                    array = emptyArray;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the item at a specific index.
    /// </summary>
    /// <param name="index">The item's index.</param>
    public T this[int index]
    {
        get
        {
            if (index >= size)
                throw new ArgumentOutOfRangeException(nameof(index));

            return array[index];
        }
        set
        {
            if (index >= size)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            array[index] = value;
        }
    }

    /// <summary>
    /// Gets or sets the item at a specific index.
    /// </summary>
    /// <param name="index">The item's index.</param>
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value!;
    }

    /// <summary>
    /// Creates a new <see cref="UnsafeList{T}"/> instance.
    /// </summary>
    public UnsafeList()
        => array = emptyArray;

    /// <summary>
    /// Creates a new <see cref="UnsafeList{T}"/> instance with a specific initial capacity.
    /// </summary>
    /// <param name="capacity">The capacity to set.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public UnsafeList(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        if (capacity == 0)
            array = emptyArray;
        else
            array = new T[capacity];
    }

    /// <summary>
    /// Creates a new <see cref="UnsafeList{T}"/> instance with items copied from a specific collection.
    /// </summary>
    /// <param name="collection">The collection to copy items from.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UnsafeList(IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        AddRange(collection);
    }

    /// <summary>
    /// Creates a new read-only collection instance.
    /// </summary>
    /// <returns>The created collection.</returns>
    public ReadOnlyCollection<T> AsReadOnly()
        => new(this);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public IEnumerator<T> GetEnumerator()
        => size == 0 ? UnsafeEnumerator<T>.emptyEnumerator : new UnsafeEnumerator<T>(this);

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        if (size < array.Length)
        {
            var curSize = size;

            size = curSize + 1;

            array[curSize] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The index of the added item.</returns>
    int IList.Add(object? item)
    {
        Add((T)item!);
        return Count - 1;
    }


    /// <summary>
    /// Adds a list of items to this collection.
    /// </summary>
    /// <param name="collection">The collection to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddRange(IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (collection is ICollection<T> c)
        {
            if (c.Count == 0)
            {
                array = emptyArray;
            }
            else
            {
                array = new T[c.Count];

                c.CopyTo(array, 0);

                size = c.Count;
            }
        }
        else
        {
            array = emptyArray;

            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Inserts an element into this list at a given index. The size of the list
    /// is increased by one. If required, the capacity of the list is doubled
    /// before inserting the new element.
    /// </summary>
    /// <param name="index">The index to insert the item into.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, T item)
    {
        // Note that insertions at the end are legal.
        if ((uint)index > (uint)size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (size == array.Length)
            GrowForInsertion(index, 1);
        else if (index < size)
            Array.Copy(array, index, array, index + 1, size - index);

        array[index] = item;

        size = size + 1;
    }

    /// <summary>
    /// Inserts an element into this list at a given index. The size of the list
    /// is increased by one. If required, the capacity of the list is doubled
    /// before inserting the new element.
    /// </summary>
    /// <param name="index">The index to insert the item into.</param>
    /// <param name="item">The item to insert.</param>
    void IList.Insert(int index, object? item)
        => Insert(index, (T)item!);

    /// <summary>
    /// Inserts the elements of the given collection at a given index. If
    /// required, the capacity of the list is increased to twice the previous
    /// capacity or the new size, whichever is larger.  Ranges may be added
    /// to the end of the list by setting index to the List's size.
    /// </summary>
    /// <param name="index">The starting index.</param>
    /// <param name="collection">The collection to insert.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if ((uint)index > (uint)size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (collection is ICollection<T> c)
        {
            if (c.Count > 0)
            {
                if (array.Length - size < c.Count)
                    GrowForInsertion(index, c.Count);
                else if (index < size)
                    Array.Copy(array, index, array, index + c.Count, size - index);

                // If we're inserting a List into itself, we want to be able to deal with that.
                if (c == this)
                {
                    // Copy first part of _items to insert location
                    Array.Copy(array, 0, array, index, index);
                    // Copy last part of _items back to inserted location
                    Array.Copy(array, index + c.Count, array, index * 2, size - index);
                }
                else
                {
                    c.CopyTo(array, index);
                }

                size = size + c.Count;
            }
        }
        else
        {
            foreach (var item in collection)
            {
                Insert(index++, item);
            }
        }
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at the end
    /// and ending at the first element in the list. The elements of the list
    /// are compared to the given value using the Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <returns>the index of the last occurrence of a given value</returns>
    public int LastIndexOf(T item)
    {
        if (size == 0)
            return -1;

        return LastIndexOf(item, size - 1, size);
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at the end
    /// and ending at the first element in the list. The elements of the list
    /// are compared to the given value using the Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>the index of the last occurrence of a given value</returns>
    public int LastIndexOf(T item, int index)
    {
        if (index >= size)
            throw new ArgumentOutOfRangeException(nameof(index));

        return LastIndexOf(item, index, index + 1);
    }

    /// <summary>
    /// Returns the index of the last occurrence of a given value in a range of
    /// this list. The list is searched backwards, starting at the end
    /// and ending at the first element in the list. The elements of the list
    /// are compared to the given value using the Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <param name="index">The starting index.</param>
    /// <param name="count">The search range size.</param>
    /// <returns>the index of the last occurrence of a given value</returns>
    public int LastIndexOf(T item, int index, int count)
    {
        if ((Count != 0) && (index < 0))
            throw new ArgumentOutOfRangeException(nameof(index));

        if ((Count != 0) && (count < 0))
            throw new ArgumentOutOfRangeException(nameof(count));

        if (size == 0)
            return -1;

        if (index >= size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count > index + 1)
            throw new ArgumentOutOfRangeException(nameof(count));

        return Array.LastIndexOf(array, item, index, count);
    }

    /// <summary>
    /// Removes the first occurrence of the given element, if found.
    /// The size of the list is decreased by one if successful.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>true if the item was removed</returns>
    public bool Remove(T item)
    {
        var index = IndexOf(item);

        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    void IList.Remove(object? item)
        => Remove((T)item!);

    /// <summary>
    /// Removes all items which matches the predicate.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The amount of removed items.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int RemoveAll(Predicate<T> match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        var freeIndex = 0; // the first free slot in items array

        // Find the first item which needs to be removed.
        while (freeIndex < size && !match(array[freeIndex]))
            freeIndex++;

        if (freeIndex >= size)
            return 0;

        var current = freeIndex + 1;

        while (current < size)
        {
            // Find the first item which needs to be kept.
            while (current < size && match(array[current]))
                current++;

            if (current < size)
            {
                // copy item to the free slot.
                array[freeIndex++] = array[current++];
            }
        }

        Array.Clear(array, freeIndex, size - freeIndex);

        var result = size - freeIndex;

        size = freeIndex;
        return result;
    }

    /// <summary>
    /// Removes the element at the given index. The size of the list is
    /// decreased by one.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)size)
            throw new ArgumentOutOfRangeException(nameof(index));

        size = size - 1;

        if (index < size)
            Array.Copy(array, index + 1, array, index, size - index);

        array[size] = default;
    }

    /// <summary>
    /// Removes a range of elements from this list.
    /// </summary>
    /// <param name="index">The starting index of the removal.</param>
    /// <param name="count">The amount of items in the removal range.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void RemoveRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count > 0)
        {
            size = size - count;

            if (index < size)
                Array.Copy(array, index + count, array, index, size - index);

            Array.Clear(array, size, count);
        }
    }

    /// <summary>
    /// Attempts to find the index of a specific item.
    /// <para> Searches a section of the list for a given element using a binary search
    /// algorithm. Elements of the list are compared to the search value using
    /// the given IComparer interface. If comparer is null, elements of
    /// the list are compared to the search value using the IComparable
    /// interface, which in that case must be implemented by all elements of the
    /// list and the given search value. This method assumes that the given
    /// section of the list is already sorted; if this is not the case, the
    /// result will be incorrect.
    /// The method returns the index of the given value in the list. If the
    /// list does not contain the given value, the method returns a negative
    /// integer. The bitwise complement operator (~) can be applied to a
    /// negative result to produce the index of the first element (if any) that
    /// is larger than the given search value. This is also the index at which
    /// the search value should be inserted into the list in order for the list
    /// to remain sorted.
    /// The method uses the Array.BinarySearch method to perform the search.</para>
    /// </summary>
    /// <param name="item">The object to search for.</param>
    /// <returns>the index of the given value in the list</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int BinarySearch(T item)
        => BinarySearch(0, Count, item, null);

    /// <summary>
    /// Attempts to find the index of a specific item.
    /// <para> Searches a section of the list for a given element using a binary search
    /// algorithm. Elements of the list are compared to the search value using
    /// the given IComparer interface. If comparer is null, elements of
    /// the list are compared to the search value using the IComparable
    /// interface, which in that case must be implemented by all elements of the
    /// list and the given search value. This method assumes that the given
    /// section of the list is already sorted; if this is not the case, the
    /// result will be incorrect.
    /// The method returns the index of the given value in the list. If the
    /// list does not contain the given value, the method returns a negative
    /// integer. The bitwise complement operator (~) can be applied to a
    /// negative result to produce the index of the first element (if any) that
    /// is larger than the given search value. This is also the index at which
    /// the search value should be inserted into the list in order for the list
    /// to remain sorted.
    /// The method uses the Array.BinarySearch method to perform the search.</para>
    /// </summary>
    /// <param name="item">The object to search for.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing elements. -or- null to use the <see cref="IComparable{T}"/> implementation of each element.</param>
    /// <returns>the index of the given value in the list</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int BinarySearch(T item, IComparer<T>? comparer)
        => BinarySearch(0, Count, item, comparer);

    /// <summary>
    /// Attempts to find the index of a specific item.
    /// <para> Searches a section of the list for a given element using a binary search
    /// algorithm. Elements of the list are compared to the search value using
    /// the given IComparer interface. If comparer is null, elements of
    /// the list are compared to the search value using the IComparable
    /// interface, which in that case must be implemented by all elements of the
    /// list and the given search value. This method assumes that the given
    /// section of the list is already sorted; if this is not the case, the
    /// result will be incorrect.
    /// The method returns the index of the given value in the list. If the
    /// list does not contain the given value, the method returns a negative
    /// integer. The bitwise complement operator (~) can be applied to a
    /// negative result to produce the index of the first element (if any) that
    /// is larger than the given search value. This is also the index at which
    /// the search value should be inserted into the list in order for the list
    /// to remain sorted.
    /// The method uses the Array.BinarySearch method to perform the search.</para>
    /// </summary>
    /// <param name="index">The starting index of the search range.</param>
    /// <param name="count">The length of the range to search.</param>
    /// <param name="item">The object to search for.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use when comparing elements. -or- null to use the <see cref="IComparable{T}"/> implementation of each element.</param>
    /// <returns>the index of the given value in the list</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int BinarySearch(int index, int count, T item, IComparer<T>? comparer = null)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        return Array.BinarySearch(array, index, count, item, comparer);
    }

    /// <summary>
    /// Checks if a specified item is contained in the collection.
    /// <para> Contains returns true if the specified element is in the List.
    /// It does a linear, O(n) search.  Equality is determined by calling
    /// EqualityComparer{T}.Default.Equals().</para>
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>true if the specified element is in the list.</returns>
    public bool Contains(T item)
    {
        // PERF: IndexOf calls Array.IndexOf, which internally
        // calls EqualityComparer<T>.Default.IndexOf, which
        // is specialized for different types. This
        // boosts performance since instead of making a
        // virtual method call each iteration of the loop,
        // via EqualityComparer<T>.Default.Equals, we
        // only make one virtual call to EqualityComparer.IndexOf.
        return size != 0 && IndexOf(item) >= 0;
    }

    /// <summary>
    /// Checks if a specified item is contained in the collection.
    /// <para> Contains returns true if the specified element is in the List.
    /// It does a linear, O(n) search.  Equality is determined by calling
    /// EqualityComparer{T}.Default.Equals().</para>
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>true if the specified element is in the list.</returns>
    bool IList.Contains(object? item)
        => Contains((T)item!);

    /// <summary>
    /// Checks if the collection contains any items that match a predicate.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>true if any items were found</returns>
    public bool Exists(Predicate<T> match)
        => FindIndex(match) != -1;

    /// <summary>
    /// Returns the index of the first occurrence of a given value in a range of
    /// this list. The list is searched forwards from beginning to end.
    /// The elements of the list are compared to the given value using the
    /// Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <returns>The index of the item (or -1 if not found).</returns>
    public int IndexOf(T item)
        => Array.IndexOf(array, item, 0, size);

    /// <summary>
    /// Returns the index of the first occurrence of a given value in a range of
    /// this list. The list is searched forwards, starting at index index and upto count number of elements.
    /// The elements of the list are compared to the given value using the
    /// Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The index of the item (or -1 if not found).</returns>
    public int IndexOf(T item, int index)
    {
        if (index > size)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Array.IndexOf(array, item, index, size - index);
    }

    /// <summary>
    /// Returns the index of the first occurrence of a given value in a range of
    /// this list. The list is searched forwards from beginning to end.
    /// The elements of the list are compared to the given value using the
    /// Object.Equals method.
    /// </summary>
    /// <param name="item">The item to find the index of.</param>
    /// <returns>The index of the item (or -1 if not found).</returns>
    int IList.IndexOf(object? item)
        => IndexOf((T)item!);

    /// <summary>
    /// Attempts to find an index of an item.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindIndex(Predicate<T> match)
        => FindIndex(0, size, match);

    /// <summary>
    /// Attempts to find an index of an item.
    /// </summary>
    /// <param name="startIndex">The search range starting index.</param>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindIndex(int startIndex, Predicate<T> match)
        => FindIndex(startIndex, size - startIndex, match);

    /// <summary>
    /// Attempts to find an index of an item.
    /// </summary>
    /// <param name="startIndex">The search range starting index.</param>
    /// <param name="count">The search range size.</param>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        if ((uint)startIndex > (uint)size)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count < 0 || startIndex > size - count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (match == null)
            throw new ArgumentNullException(nameof(match));

        var endIndex = startIndex + count;

        for (var i = startIndex; i < endIndex; i++)
        {
            if (match(array[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Attempts to find an index of an item, starting from the end of the collection.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindLastIndex(Predicate<T> match)
        => FindLastIndex(size - 1, size, match);

    /// <summary>
    /// Attempts to find an index of an item, starting from the end of the collection.
    /// </summary>
    /// <param name="startIndex">The search range starting index.</param>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindLastIndex(int startIndex, Predicate<T> match)
        => FindLastIndex(startIndex, startIndex + 1, match);

    /// <summary>
    /// Attempts to find an index of an item, starting from the end of the collection.
    /// </summary>
    /// <param name="startIndex">The search range starting index.</param>
    /// <param name="count">The search range size.</param>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The index of the matched item or -1.</returns>
    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        if (size == 0)
        {
            // Special case for 0 length List
            if (startIndex != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
        }
        else
        {
            // Make sure we're not out of range
            if ((uint)startIndex >= (uint)size)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
        }

        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
        if (count < 0 || startIndex - count + 1 < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var endIndex = startIndex - count;

        for (int i = startIndex; i > endIndex; i--)
        {
            if (match(array[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Attempts to find an item matching a predicate.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The item instance if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T? Find(Predicate<T> match)
    {
        if (match is null)
            throw new ArgumentNullException(nameof(match));

        for (var i = 0; i < size; i++)
        {
            var item = array[i];

            if (match(item))
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    /// Attempts to find an item matching a predicate, starting from the end of the collection.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>The matched item.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T? FindLast(Predicate<T> match)
    {
        if (match is null)
            throw new ArgumentNullException(nameof(match));

        for (var i = size - 1; i >= 0; i--)
        {
            var item = array[i];

            if (match(item))
            {
                return item;
            }
        }

        return default;
    }

    /// <summary>
    /// Finds all items matching a predicate.
    /// </summary>
    /// <param name="match">The predicate to match.</param>
    /// <returns>A list of items matching the predicate.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<T> FindAll(Predicate<T> match)
    {
        if (match is null)
            throw new ArgumentNullException(nameof(match));

        var list = new List<T>();

        for (var i = 0; i < size; i++)
        {
            var item = array[i];

            if (match(item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    /// <summary>
    /// Invokes a delegate on each item in the collection.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void ForEach(Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        for (var i = 0; i < size; i++)
            action(array[i]);
    }


    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        if (size > 0)
            Array.Clear(array, 0, size);

        size = 0;
    }

    /// <summary>
    /// Gets a segment of array range.
    /// </summary>
    /// <param name="count">The size of the segment's range.</param>
    /// <returns>The created segment.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ArraySegment<T> GetSegment(int count)
        => GetSegment(0, count);

    /// <summary>
    /// Gets a segment of array range.
    /// </summary>
    /// <param name="startIndex">The starting index of the segment's range.</param>
    /// <param name="count">The size of the segment's range.</param>
    /// <returns>The created segment.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ArraySegment<T> GetSegment(int startIndex, int count)
    {
        if (startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        return new(array, startIndex, count);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="UnsafeList{T}" />.
    /// </summary>
    /// <param name="start">The zero-based <see cref="UnsafeList{T}" /> index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A shallow copy of a range of elements in the source <see cref="UnsafeList{T}" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start" /> is less than 0.
    /// -or-
    /// <paramref name="length" /> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="start" /> and <paramref name="length" /> do not denote a valid range of elements in the <see cref="UnsafeList{T}" />.</exception>
    public UnsafeList<T> Slice(int start, int length)
        => GetRange(start, length);

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="UnsafeList{T}" />.
    /// </summary>
    /// <param name="index">The zero-based <see cref="UnsafeList{T}" /> index at which the range starts.</param>
    /// <param name="count">The length of the range.</param>
    /// <returns>A shallow copy of a range of elements in the source <see cref="UnsafeList{T}" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than 0.
    /// -or-
    /// <paramref name="count" /> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in the <see cref="UnsafeList{T}" />.</exception>
    public UnsafeList<T> GetRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        var list = new UnsafeList<T>(count);

        Array.Copy(array, index, list.array, 0, count);

        list.size = count;
        return list;
    }

    /// <summary>
    /// Returns a new collection instance with items converted to a different type.
    /// </summary>
    /// <param name="converter">The delegate used to convert items.</param>
    /// <typeparam name="TOutput">The type to convert the items to.</typeparam>
    /// <returns>The created collection instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public UnsafeList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        if (converter is null)
            throw new ArgumentNullException(nameof(converter));

        var output = new UnsafeList<TOutput>(size);

        for (var i = 0; i < size; i++)
            output.array[i] = converter(array[i]);

        output.size = size;
        return output;
    }


    /// <summary>
    /// Copies this list into array, which must be of a compatible array type.
    /// </summary>
    /// <param name="array">The array to copy the items to.</param>
    public void CopyTo(T[] array)
        => CopyTo(array, 0);

    /// <summary>
    /// Copies this list into array, which must be of a compatible array type.
    /// </summary>
    /// <param name="array">The array to copy the items to.</param>
    /// <param name="arrayIndex">The index offset of the target array.</param>
    void ICollection.CopyTo(Array array, int arrayIndex)
        => Array.Copy(this.array, 0, array!, arrayIndex, size);

    /// <summary>
    /// Copies this list into array, which must be of a compatible array type.
    /// </summary>
    /// <param name="index">The index offset of this collection.</param>
    /// <param name="array">The array to copy the items to.</param>
    /// <param name="arrayIndex">The index offset of the target array.</param>
    /// <param name="count">The amount of items to copy.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        Array.Copy(this.array, index, array, arrayIndex, count);
    }

    /// <summary>
    /// Copies this list into array, which must be of a compatible array type.
    /// </summary>
    /// <param name="array">The array to copy the items to.</param>
    /// <param name="arrayIndex">The index offset of the target array.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void CopyTo(T[] array, int arrayIndex)
        => Array.Copy(this.array, 0, array, arrayIndex, size);

    /// <summary>
    /// Ensures that the capacity of this list is at least the specified <paramref name="capacity"/>.
    /// If the current capacity of the list is less than specified <paramref name="capacity"/>,
    /// the capacity is increased to at least <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this list.</returns>
    public int EnsureCapacity(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        if (array.Length < capacity)
            Grow(capacity);

        return array.Length;
    }

    /// <summary>
    /// Reverses the elements in a range of this list. Following a call to this
    /// method, an element in the range given by index and count
    /// which was previously located at index i will now be located at
    /// index index + (index + count - i - 1).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Reverse()
        => Reverse(0, Count);
    
    /// <summary>
    /// Reverses the elements in a range of this list. Following a call to this
    /// method, an element in the range given by index and count
    /// which was previously located at index i will now be located at
    /// index index + (index + count - i - 1).
    /// </summary>
    /// <param name="index">The reversal range starting index.</param>
    /// <param name="count">The reversal range length.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Reverse(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count > 1)
            Array.Reverse(array, index, count);
    }

    /// <summary>
    /// Sorts the elements in a section of this list. The sort compares the
    /// elements to each other using the given IComparer interface. If
    /// comparer is null, the elements are compared to each other using
    /// the IComparable interface, which in that case must be implemented by all
    /// elements of the list.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Sort()
        => Sort(0, Count, null);

    /// <summary>
    /// Sorts the elements in a section of this list. The sort compares the
    /// elements to each other using the given IComparer interface. If
    /// comparer is null, the elements are compared to each other using
    /// the IComparable interface, which in that case must be implemented by all
    /// elements of the list.
    /// </summary>
    /// <param name="comparer">The comparer used to compare elements.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Sort(IComparer<T>? comparer)
        => Sort(0, Count, comparer);
    
    /// <summary>
    /// Sorts the elements in a section of this list. The sort compares the
    /// elements to each other using the given IComparer interface. If
    /// comparer is null, the elements are compared to each other using
    /// the IComparable interface, which in that case must be implemented by all
    /// elements of the list.
    /// </summary>
    /// <param name="index">The sorting range start index.</param>
    /// <param name="count">The sorting range length.</param>
    /// <param name="comparer">The comparer used to compare elements.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count > 1)
            Array.Sort(array, index, count, comparer);
    }

    /// <summary>
    /// Accesses the underlying array.
    /// </summary>
    /// <returns>The underlying array.</returns>
    public T[] AccessArray()
        => array;
    
    /// <summary>
    /// Returns an array containing the contents of the List.
    /// </summary>
    /// <returns>The new created array.</returns>
    public T[] ToArray()
    {
        if (size == 0)
            return emptyArray;

        var array = new T[size];
        
        Array.Copy(this.array, array, size);
        return array;
    }
    
    /// <summary>
    /// Sets the capacity of this list to the size of the list. This method can
    /// be used to minimize a list's memory overhead once it is known that no
    /// new elements will be added to the list.
    /// </summary>
    public void TrimExcess()
    {
        var threshold = (int)(((double)array.Length) * 0.9);
        
        if (size < threshold)
            Capacity = size;
    }

    /// <summary>
    /// Whether or not a specific condition is true for all items in the collection.
    /// </summary>
    /// <param name="match">The condition.</param>
    /// <returns>true if the condition is true for all the items in the collection</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TrueForAll(Predicate<T> match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        for (var i = 0; i < size; i++)
        {
            if (!match(array[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void AddWithResize(T item)
    {
        var curSize = size;

        Grow(size + 1);

        size = curSize + 1;

        array[curSize] = item;
    }
    
    private void Grow(int capacity)
        => Capacity = GetNewCapacity(capacity);
    
    private void GrowForInsertion(int indexToInsert, int insertionCount = 1)
    {
        var requiredCapacity = checked(size + insertionCount);
        var newCapacity = GetNewCapacity(requiredCapacity);

        // Inline and adapt logic from set_Capacity

        var newItems = new T[newCapacity];
        
        if (indexToInsert != 0)
            Array.Copy(array, newItems, length: indexToInsert);

        if (size != indexToInsert)
            Array.Copy(array, indexToInsert, newItems, indexToInsert + insertionCount, size - indexToInsert);

        array = newItems;
    }
    
    private int GetNewCapacity(int capacity)
    {
        var newCapacity = array.Length == 0 ? 4 : 2 * array.Length;

        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > MaxArrayLength)
            newCapacity = MaxArrayLength;

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newCapacity < capacity) 
            newCapacity = capacity;

        return newCapacity;
    }
}