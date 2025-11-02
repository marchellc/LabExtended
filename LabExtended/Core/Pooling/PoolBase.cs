using LabExtended.API;
using LabExtended.Extensions;

namespace LabExtended.Core.Pooling;

/// <summary>
/// Provides a base class for implementing object pools that manage reusable instances of a specified reference type.
/// </summary>
/// <remarks>This class defines the core functionality for pooling objects, including methods for renting,
/// returning, preloading, and clearing pooled instances. Derived classes should implement the abstract members to
/// provide pool-specific behavior, such as naming or custom handling of pooled items. The pool is not thread-safe by
/// default; synchronization must be handled externally if used in multithreaded scenarios. Implements IDisposable to
/// allow for explicit resource cleanup.</remarks>
/// <typeparam name="T">The type of objects to be managed by the pool. Must be a reference type.</typeparam>
public abstract class PoolBase<T> : IDisposable where T : class
{
    private volatile List<T> _pool;

    /// <summary>
    /// Initializes a new instance of the PoolBase class.
    /// </summary>
    public PoolBase()
        => _pool = new List<T>();
    
    /// <summary>
    /// Initializes a new instance of the pool with the specified capacity and optionally preloads it with items.
    /// </summary>
    /// <remarks>If both <paramref name="initialSize"/> and <paramref name="initialConstructor"/> are
    /// provided, the pool is pre-populated with the specified number of items created by the constructor function.
    /// Otherwise, the pool is initialized empty.</remarks>
    /// <param name="size">The maximum number of items the pool can hold. Must be greater than zero.</param>
    /// <param name="initialSize">The number of items to preload into the pool. If specified, must be less than or equal to <paramref
    /// name="size"/>.</param>
    /// <param name="initialConstructor">A function used to create each preloaded item. Required if <paramref name="initialSize"/> is specified.</param>
    public PoolBase(int size, int? initialSize = null, Func<T> initialConstructor = null)
    {
        _pool = new List<T>(size);

        if (initialSize.HasValue && initialConstructor != null)
            Preload(initialSize.Value, initialConstructor);
    }
    
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.Now;

    /// <summary>
    /// Gets the number of items currently contained in the pool.
    /// </summary>
    public int Size => _pool.Count;
    
    /// <summary>
    /// Gets the name associated with the current instance.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Preloads the pool with a specified number of items created by the provided constructor function.
    /// </summary>
    /// <remarks>If the specified size is less than 1, the method does nothing. The pool is cleared before
    /// preloading, and its capacity is increased if necessary. Items are only added if successfully created by the
    /// constructor and while the server is running.</remarks>
    /// <param name="size">The number of items to preload into the pool. Must be greater than or equal to 1.</param>
    /// <param name="constructor">A function used to create new instances of type T to populate the pool. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the constructor parameter is null.</exception>
    public void Preload(int size, Func<T> constructor)
    {
        if (size < 1)
            return;

        if (constructor is null)
            throw new ArgumentNullException(nameof(constructor));

        if (_pool.Capacity < size)
            _pool.Capacity = size;

        _pool.Clear();

        while (_pool.Count != size && ExServer.IsRunning)
        {
            var item = constructor();
            
            if (item is null)
                continue;

            HandleNewItem(item);
            HandleReturn(item);
            
            _pool.Add(item);
        }
    }

    /// <summary>
    /// Rents an instance of type T from the pool, optionally applying a setup action or using a custom constructor if
    /// the pool is empty.
    /// </summary>
    /// <remarks>If the pool contains available instances, one is removed and returned. If the pool is empty,
    /// a new instance is created using the provided constructor. The setup action, if specified, is invoked on the
    /// instance before it is returned.</remarks>
    /// <param name="setup">An optional action to configure the rented instance before it is returned. If null, no setup is performed.</param>
    /// <param name="constructor">An optional function used to create a new instance of type T if the pool is empty. Must not be null when the
    /// pool is empty.</param>
    /// <returns>An instance of type T, either taken from the pool or newly created using the specified constructor.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the pool is empty and the constructor parameter is null.</exception>
    /// <exception cref="Exception">Thrown if the constructor fails to create a valid instance of type T.</exception>
    public T Rent(Action<T>? setup = null, Func<T>? constructor = null)
    {
        if (_pool.Count < 1)
        {
            if (constructor is null)
                throw new ArgumentNullException(nameof(constructor));

            var value = constructor();

            if (value is null)
                throw new Exception($"Constructor failed to create instance");

            HandleNewItem(value);
            
            setup?.InvokeSafe(value);

            HandleRent(value);
            return value;
        }
        else
        {
            var value = _pool.RemoveAndTake(0);

            setup?.InvokeSafe(value);

            HandleRent(value);
            return value;
        }
    }

    /// <summary>
    /// Returns an object to the pool for future reuse, optionally performing cleanup on the object before it is
    /// returned.
    /// </summary>
    /// <remarks>Use this method to return objects that were previously obtained from the pool. Providing a
    /// cleanup action allows you to reset or clear the object's state before it is reused by other callers.</remarks>
    /// <param name="value">The object to return to the pool. Cannot be null.</param>
    /// <param name="cleanValue">An optional action to perform cleanup on the object before it is returned to the pool. If null, no cleanup is
    /// performed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public void Return(T value, Action<T>? cleanValue = null)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        cleanValue?.InvokeSafe(value);

        HandleReturn(value);
        
        _pool.Add(value);
    }

    /// <summary>
    /// Removes all objects from the pool, optionally invoking a specified action on each instance before removal.
    /// </summary>
    /// <remarks>Use this method to release all pooled objects and optionally perform cleanup or disposal
    /// logic on each instance. After calling this method, the pool will be empty.</remarks>
    /// <param name="destroyInstance">An optional action to perform on each pooled instance before it is removed. If null, no action is taken.</param>
    /// <exception cref="Exception">Thrown if the internal pool is not initialized.</exception>
    public void Clear(Action<T>? destroyInstance = null)
    {
        if (_pool is null)
            throw new Exception("Internal pool is null");
        
        if (destroyInstance != null)
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                destroyInstance.InvokeSafe(_pool[i]);
            }
        }

        _pool.Clear();
    }

    /// <summary>
    /// Releases all resources used by the current instance of the class.
    /// </summary>
    /// <remarks>Call this method when you are finished using the object to release unmanaged resources and
    /// perform other cleanup operations. After calling Dispose, the object should not be used further.</remarks>
    public void Dispose() 
        => Dispose(null);

    /// <summary>
    /// Releases all resources used by the pool and optionally destroys each pooled instance using the specified action.
    /// </summary>
    /// <remarks>After calling this method, the pool is cleared and cannot be used unless reinitialized. If a
    /// destroy action is provided, it is invoked for each instance in the pool before disposal. This method is not
    /// thread-safe.</remarks>
    /// <param name="destroyInstance">An optional action to perform on each pooled instance before the pool is cleared. If null, no action is taken on
    /// the instances.</param>
    public void Dispose(Action<T>? destroyInstance = null)
    {
        if (_pool != null)
        {
            if (destroyInstance != null)
            {
                for (int i = 0; i < _pool.Count; i++)
                {
                    destroyInstance?.InvokeSafe(_pool[i]);
                }
            }

            _pool.Clear();
            _pool = null!;
        }
    }

    /// <summary>
    /// Handles a newly added item of type T. Intended to be overridden to provide custom processing when a new item is
    /// introduced.
    /// </summary>
    /// <remarks>The default implementation performs no action. Override this method in a derived class to
    /// implement custom handling logic for new items.</remarks>
    /// <param name="item">The item to be handled. May be null if the type T is a reference type.</param>
    public virtual void HandleNewItem(T item) { }

    /// <summary>
    /// Handles the return of an item to the pool or resource manager.
    /// </summary>
    /// <param name="item">The item to be returned. The behavior depends on the implementation and the type of item.</param>
    public virtual void HandleReturn(T item) { }

    /// <summary>
    /// Handles the rental process for the specified item.
    /// </summary>
    /// <param name="item">The item to be rented. Cannot be null.</param>
    public virtual void HandleRent(T item) { }
}