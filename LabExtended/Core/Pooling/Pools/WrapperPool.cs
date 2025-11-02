using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

/// <summary>
/// Provides a thread-safe object pool for reusing instances of <see cref="PoolWrapper{T}"/> objects.
/// </summary>
/// <remarks>Use <see cref="WrapperPool{T}.Shared"/> to access a shared instance of the pool. This class is
/// intended to reduce allocation overhead by reusing wrapper objects for the specified type.</remarks>
/// <typeparam name="T">The type of object to be wrapped and pooled.</typeparam>
public class WrapperPool<T> : ObjectPool<PoolWrapper<T>>
{
    private static volatile WrapperPool<T> _shared;

    /// <summary>
    /// Gets a shared instance of the wrapper pool for the specified type.
    /// </summary>
    /// <remarks>Use this property to access a singleton pool instance that can be shared across multiple
    /// components or threads. The shared pool is lazily initialized on first access and is intended for general-purpose
    /// reuse scenarios.</remarks>
    public static new WrapperPool<T> Shared => _shared ??= new WrapperPool<T>();

    /// <summary>
    /// Rents an object wrapper from the pool, optionally applying a setup action and using a custom constructor.
    /// </summary>
    /// <remarks>The setup action is invoked each time an object is rented from the pool. Supplying a custom
    /// constructor allows for control over how new objects are created when the pool is empty.</remarks>
    /// <param name="setup">An optional action to perform on the pooled object after it is retrieved. If null, no setup is performed.</param>
    /// <param name="constructor">An optional function used to create a new instance of the pooled object if the pool is empty. If null, the
    /// default constructor is used.</param>
    /// <returns>A PoolWrapper{T} containing the rented object. The wrapper should be disposed or returned to the pool when no
    /// longer needed.</returns>
    public PoolWrapper<T> Rent(Action<T>? setup = null, Func<T>? constructor = null)
        => base.Rent(wrapper => setup?.InvokeSafe(wrapper.Value), () => new PoolWrapper<T>(constructor()));
}