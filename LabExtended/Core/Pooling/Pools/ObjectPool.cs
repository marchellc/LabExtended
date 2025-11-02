namespace LabExtended.Core.Pooling.Pools;

/// <summary>
/// Provides a pool for managing reusable objects of type T, enabling efficient allocation and reuse to minimize object
/// creation overhead.
/// </summary>
/// <remarks>ObjectPool<T> is typically used to reduce memory allocations and improve performance in scenarios
/// where objects are frequently created and destroyed. The pool automatically manages the lifecycle of pooled objects,
/// invoking appropriate callbacks when objects are rented, returned, or constructed. Use the Shared property to access
/// a thread-safe, shared instance of the pool for a given type.</remarks>
/// <typeparam name="T">The type of objects managed by the pool. Must inherit from PoolObject.</typeparam>
public class ObjectPool<T> : PoolBase<T>
    where T : PoolObject
{
    private static volatile ObjectPool<T> _shared;

    /// <summary>
    /// Gets a shared instance of the object pool for the specified type.
    /// </summary>
    /// <remarks>The shared instance is lazily initialized and intended for general-purpose use. It is
    /// thread-safe and can be used concurrently across multiple threads. Use this property when a single,
    /// application-wide object pool is sufficient for your scenario.</remarks>
    public static ObjectPool<T> Shared => _shared ??= new ObjectPool<T>();

    /// <summary>
    /// Gets the display name of the object pool instance.
    /// </summary>
    /// <remarks>The name includes the type parameter of the object pool, which can be useful for logging or
    /// diagnostics.</remarks>
    public override string Name { get; } = $"ObjectPool<{typeof(T).Name}>";

    /// <summary>
    /// Handles the process of renting an item from the pool.
    /// </summary>
    /// <remarks>This method updates the item's state to indicate it is no longer pooled and invokes any
    /// custom logic associated with renting the item. Overrides the base implementation to provide additional handling
    /// specific to the item type.</remarks>
    /// <param name="item">The item to be rented from the pool. Cannot be null.</param>
    public override void HandleRent(T item)
    {
        base.HandleRent(item);
        
        item.IsPooled = false;
        item.OnRented();
    }

    /// <summary>
    /// Handles the return of an item to the pool, marking it as available for reuse.
    /// </summary>
    /// <remarks>After this method is called, the item is considered available for subsequent retrievals from
    /// the pool. The item's state is updated to reflect that it is pooled, and any custom return logic defined by the
    /// item is executed.</remarks>
    /// <param name="item">The item being returned to the pool. Must not be null.</param>
    public override void HandleReturn(T item)
    {
        base.HandleReturn(item);
        
        item.IsPooled = true;
        item.OnReturned();
    }

    /// <summary>
    /// Initializes a newly added item by associating it with this pool and invoking its construction logic.
    /// </summary>
    /// <remarks>This method sets the item's pool reference and calls its construction handler. Override this
    /// method to customize initialization behavior for new items.</remarks>
    /// <param name="item">The item to be initialized and added to the pool. Cannot be null.</param>
    public override void HandleNewItem(T item)
    {
        base.HandleNewItem(item);
        
        item.PoolInstance = this;
        item.OnConstructed();
    }
}