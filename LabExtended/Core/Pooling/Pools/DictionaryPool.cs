using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

/// <summary>
/// Provides a pool for reusing instances of <see cref="Dictionary{TKey, TValue}"/> to minimize allocations and improve
/// performance in scenarios with frequent dictionary usage.
/// </summary>
/// <remarks>Use this pool to rent and return <see cref="Dictionary{TKey, TValue}"/> instances instead of creating
/// and discarding them repeatedly. This can reduce memory pressure and garbage collection overhead in high-throughput
/// or performance-sensitive applications. Returned dictionaries are cleared before being reused. The <see
/// cref="Shared"/> property provides a thread-safe, shared instance suitable for most scenarios.</remarks>
/// <typeparam name="TKey">The type of keys in the pooled dictionaries.</typeparam>
/// <typeparam name="TValue">The type of values in the pooled dictionaries.</typeparam>
public class DictionaryPool<TKey, TValue> : PoolBase<Dictionary<TKey, TValue>>
{
    private static volatile DictionaryPool<TKey, TValue> _shared;

    /// <summary>
    /// Gets a shared instance of the dictionary pool for the specified key and value types.
    /// </summary>
    /// <remarks>Use this property to access a thread-safe, shared pool of dictionaries for the given generic
    /// types. The shared instance is lazily initialized and can be reused across multiple components or threads to
    /// reduce memory allocations.</remarks>
    public static DictionaryPool<TKey, TValue> Shared => _shared ??= new DictionaryPool<TKey, TValue>();

    /// <summary>
    /// Gets the display name of the dictionary pool, including the key and value type names.
    /// </summary>
    public override string Name { get; } = $"DictionaryPool<{typeof(TKey).Name}, {typeof(TValue).Name}>";

    /// <summary>
    /// Rents a dictionary instance from the pool for temporary use.
    /// </summary>
    /// <remarks>The rented dictionary should be returned to the pool when no longer needed, following the
    /// pool's usage guidelines. This method is intended to reduce allocations by reusing dictionary
    /// instances.</remarks>
    /// <returns>A dictionary instance of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/>. The returned
    /// dictionary may contain data from previous usage and should be cleared before use if necessary.</returns>
    public Dictionary<TKey, TValue> Rent()
        => base.Rent(null, () => new Dictionary<TKey, TValue>());

    /// <summary>
    /// Rents a dictionary instance with the specified initial capacity from the pool.
    /// </summary>
    /// <remarks>The rented dictionary is not cleared before being returned. Callers are responsible for
    /// clearing and returning the dictionary to the pool after use to avoid memory leaks.</remarks>
    /// <param name="capacity">The initial number of elements that the returned dictionary can contain. Must be non-negative.</param>
    /// <returns>A dictionary instance with the specified initial capacity. The returned dictionary should be returned to the
    /// pool when no longer needed.</returns>
    public Dictionary<TKey, TValue> Rent(int capacity)
        => base.Rent(null, () => new Dictionary<TKey, TValue>(capacity));

    /// <summary>
    /// Rents a dictionary from the pool and populates it with the entries from the specified dictionary.
    /// </summary>
    /// <remarks>The returned dictionary is intended for temporary use and should be returned to the pool
    /// according to the pool's usage guidelines. Modifying the returned dictionary does not affect the original
    /// dictionary.</remarks>
    /// <param name="dictionary">The source dictionary whose key-value pairs are copied into the rented dictionary. Cannot be null.</param>
    /// <returns>A dictionary instance containing the same key-value pairs as the specified dictionary. The returned dictionary
    /// must be returned to the pool when no longer needed.</returns>
    public Dictionary<TKey, TValue> Rent(IDictionary<TKey, TValue> dictionary)
        => base.Rent(x => x.AddRange(dictionary), () => new Dictionary<TKey, TValue>(dictionary));

    /// <summary>
    /// Returns a dictionary instance to the pool for reuse after clearing its contents.
    /// </summary>
    /// <remarks>After this method is called, the contents of the dictionary are cleared. The dictionary
    /// instance should not be used after it has been returned to the pool.</remarks>
    /// <param name="dictionary">The dictionary instance to return to the pool. If null, no action is taken.</param>
    public void Return(Dictionary<TKey, TValue>? dictionary)
        => base.Return(dictionary!, x => x.Clear());
}