namespace LabExtended.Core.Pooling;

/// <summary>
/// Provides a wrapper for an object obtained from a pool, ensuring the object can be returned to the pool when
/// disposed.
/// </summary>
/// <remarks>PoolWrapper{T} is typically used to manage the lifetime of pooled objects in a using statement.
/// Disposing the wrapper returns the object to the pool. This class is not thread-safe.</remarks>
/// <typeparam name="T">The type of the object managed by the pool.</typeparam>
public class PoolWrapper<T> : PoolObject, IDisposable
{
    /// <summary>
    /// Gets the value contained by the current instance.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the PoolWrapper{T} class with the specified value.
    /// </summary>
    /// <param name="value">The value to be wrapped by the PoolWrapper{T} instance.</param>
    public PoolWrapper(T value)
        => Value = value;

    /// <summary>
    /// Releases resources used by the object and returns it to the pool for reuse.
    /// </summary>
    /// <remarks>Call this method when the object is no longer needed to make it available for pooling. After
    /// calling this method, the object should not be used unless it is obtained from the pool again.</remarks>
    public void Dispose()
        => ReturnToPool<PoolWrapper<T>>();
}