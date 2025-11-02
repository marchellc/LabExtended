namespace LabExtended.Core.Pooling;

/// <summary>
/// Represents an object that can be managed by an object pool, providing lifecycle hooks and pool association
/// information.
/// </summary>
/// <remarks>PoolObject serves as a base class for objects that participate in pooling scenarios. It provides
/// properties to track pool membership and methods that can be overridden to respond to key lifecycle events, such as
/// construction, rental, and return to the pool. This class is intended to be used in conjunction with a pooling
/// framework that manages the allocation and recycling of objects to improve performance and resource usage.</remarks>
public class PoolObject
{
    /// <summary>
    /// Gets the underlying object instance managed by the pool.
    /// </summary>
    /// <remarks>This property is intended for advanced scenarios where direct access to the pooled object is
    /// required. Modifying the returned instance may affect other consumers of the pool.</remarks>
    public object PoolInstance { get; internal set; }
    
    /// <summary>
    /// Gets a value indicating whether the object is managed by a pool.
    /// </summary>
    public bool IsPooled { get; internal set; }

    /// <summary>
    /// Returns the current object to its associated object pool of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to return to the pool. Must be a reference type.</typeparam>
    /// <exception cref="Exception">Thrown if the current object is not associated with a pool of type T.</exception>
    public void ReturnToPool<T>() where T : class
    {
        if (IsPooled || PoolInstance is null)
            return;

        if (PoolInstance is not PoolBase<T> poolBase)
            throw new Exception($"Invalid generic type: {typeof(T).FullName}");

        poolBase.Return((T)(object)this);
    }
    
    /// <summary>
    /// Called after the object has been constructed to perform additional initialization.
    /// </summary>
    /// <remarks>Override this method in a derived class to execute custom logic immediately after
    /// construction. This method is intended to be called once during the object's lifecycle, typically by the
    /// framework or infrastructure code.</remarks>
    public virtual void OnConstructed() { }

    /// <summary>
    /// Called when an object is returned to the pool. Override this method to perform custom actions when the object is
    /// released.
    /// </summary>
    /// <remarks>This method is intended to be overridden in a derived class to implement logic that should
    /// occur when an object is returned to the pool, such as resetting state or releasing resources. The default
    /// implementation does nothing.</remarks>
    public virtual void OnReturned() { }

    /// <summary>
    /// Called when the object is obtained from the pool. Override this method to perform custom actions when the object
    /// is rented.
    /// </summary>
    /// <remarks>This method is intended to be overridden in a derived class to implement logic that should
    /// occur each time the object is rented from the pool. The default implementation does nothing.</remarks>
    public virtual void OnRented() { }
}