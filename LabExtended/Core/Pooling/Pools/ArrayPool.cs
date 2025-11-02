using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

/// <summary>
/// Provides a resource pool for arrays of a specified type, enabling efficient reuse of array instances to minimize
/// memory allocations.
/// </summary>
/// <remarks>ArrayPool<T> is useful for scenarios where arrays are frequently allocated and released, such as in
/// high-performance or memory-sensitive applications. By reusing arrays, it helps reduce garbage collection pressure
/// and improve application performance. Arrays obtained from the pool should be returned after use to avoid memory
/// leaks. This class is not thread-safe; external synchronization is required if used concurrently from multiple
/// threads.</remarks>
/// <typeparam name="T">The type of elements stored in the arrays managed by the pool.</typeparam>
public class ArrayPool<T>
{
    private static volatile ArrayPool<T> _shared;

    /// <summary>
    /// Gets a shared instance of the array pool for the specified type.
    /// </summary>
    /// <remarks>The shared pool is intended for general-purpose use and is thread-safe. It is suitable for
    /// most scenarios where pooling of arrays is required. Applications that require custom pooling behavior should
    /// create their own instance of ArrayPool{T}.</remarks>
    public static ArrayPool<T> Shared => _shared ??= new ArrayPool<T>();

    private Dictionary<int, List<T[]>> _pool = new Dictionary<int, List<T[]>>();

    /// <summary>
    /// Rents an array of the specified size from the pool, or creates a new array if none are available.
    /// </summary>
    /// <remarks>If an array of the requested size is available in the pool, it is returned; otherwise, a new
    /// array is created. If clearArray is set to true, all elements in the returned array are reset to their default
    /// values.</remarks>
    /// <param name="size">The number of elements in the array to rent. Must be greater than 0.</param>
    /// <param name="clearArray">true to clear the contents of the array before returning it; otherwise, false.</param>
    /// <returns>An array of type T with the specified length. The array may be newly allocated or previously used.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if size is less than 1.</exception>
    public T[] Rent(int size, bool clearArray = false)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        
        if (!_pool.TryGetValue(size, out var list))
            _pool[size] = list = new List<T[]>();

        if (list.Count < 1)
            return new T[size];

        var array = list.RemoveAndTake(0);

        if (clearArray)
            Array.Clear(array, 0, array.Length);

        return array;
    }

    /// <summary>
    /// Returns the specified array to the pool for reuse.
    /// </summary>
    /// <remarks>If cleanArray is set to true, all elements of the array are reset to their default values
    /// before the array is returned to the pool. This helps prevent data leakage between usages. If cleanArray is
    /// false, the contents of the array are left unchanged.</remarks>
    /// <param name="array">The array to return to the pool. Cannot be null.</param>
    /// <param name="cleanArray">true to clear the contents of the array before returning it to the pool; otherwise, false.</param>
    /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
    public void Return(T[] array, bool cleanArray = true)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        var size = array.Length;

        if (cleanArray)
            Array.Clear(array, 0, size);

        if (_pool.TryGetValue(size, out var list))
            list.Add(array);
        else
            _pool[size] = new List<T[]>() { array };
    }
}