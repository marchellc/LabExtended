using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Pooling;

/// <summary>
/// Used for pooling class instances.
/// </summary>
public class InstancePool<T> : IDisposable
{
    /// <summary>
    /// Gets the pool's queue.
    /// </summary>
    public List<T> Pool { get; private set; } = ListPool<T>.Shared.Rent(byte.MaxValue);

    /// <summary>
    /// Rents an instance from the pool.
    /// </summary>
    /// <param name="factory">The instance factory delegate.</param>
    /// <returns>The rented or created instance.</returns>
    public T Rent(Func<T> factory)
    {
        if (Pool.Count > 0)
            return Pool.RemoveAndTake(0);
        
        return factory();
    }

    /// <summary>
    /// Returns an instance to the pool.
    /// </summary>
    /// <param name="item">The instance to return.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Return(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        
        Pool.Add(item);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Pool != null)
            ListPool<T>.Shared.Return(Pool);

        Pool = null;
    }
}