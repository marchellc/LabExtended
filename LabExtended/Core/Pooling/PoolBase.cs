using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Pooling;

public abstract class PoolBase<T> : IDisposable
    where T : class
{
    public static unsafe int TypeSize { get; } = sizeof(T);
    
    private volatile List<T> _pool;

    public PoolBase()
    {
        _pool = new List<T>();
        PoolManager.Register(this);
    }


    public PoolBase(int size, int? initialSize = null, Func<T> initialConstructor = null)
    {
        _pool = new List<T>(size);

        if (initialSize.HasValue && initialConstructor != null)
            Preload(initialSize.Value, initialConstructor);

        PoolManager.Register(this);
    }
    
    public DateTime CreatedAt { get; } = DateTime.Now;

    public int Size => _pool.Count;
    public int Id { get; internal set; }
    
    public abstract string Name { get; }

    public void Preload(int size, Func<T> constructor)
    {
        if (size < 1)
            return;

        if (constructor is null)
            throw new ArgumentNullException(nameof(constructor));

        if (_pool.Capacity < size)
            _pool.Capacity = size;

        _pool.Clear();

        while (_pool.Count != size)
        {
            var item = constructor();
            
            if (item is null)
                continue;

            HandleNewItem(item);
            HandleReturn(item);
            
            _pool.Add(item);
        }
    }

    public T Rent(Action<T> setup = null, Func<T> constructor = null)
    {
        if (_pool.Count < 1)
        {
            if (constructor is null)
                throw new ArgumentNullException(nameof(constructor));

            var value = constructor();

            if (value is null)
                throw new Exception($"Constructor failed to create instance");

            HandleNewItem(value);
            
            setup.InvokeSafe(value);

            HandleRent(value);
            return value;
        }
        else
        {
            var value = _pool.RemoveAndTake(0);

            setup.InvokeSafe(value);

            HandleRent(value);
            return value;
        }
    }

    public void Return(T value, Action<T> cleanValue = null)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        cleanValue.InvokeSafe(value);

        HandleReturn(value);
        
        _pool.Add(value);
    }

    public void Clear(Action<T> destroyInstance = null)
    {
        if (_pool is null)
            throw new Exception("Internal pool is null");
        
        if (destroyInstance != null)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                destroyInstance.InvokeSafe(_pool[i]);
            }
        }

        _pool.Clear();
    }

    public void Dispose() => Dispose(null);

    public void Dispose(Action<T> destroyInstance = null)
    {
        if (_pool != null)
        {
            if (destroyInstance != null)
            {
                for (int i = 0; i < _pool.Count; i++)
                {
                    destroyInstance.InvokeSafe(_pool[i]);
                }
            }

            _pool.Clear();
            _pool = null;

            PoolManager.Unregister(this);
        }
    }

    public virtual void HandleNewItem(T item) { }
    public virtual void HandleReturn(T item) { }
    public virtual void HandleRent(T item) { }

    public override string ToString()
        => StringBuilderPool.Shared.BuildString(x =>
        {
            x.Append(Name);
            x.Append(" (ID: ");
            x.Append(Id);
            x.Append(")");

            if (_pool is null)
            {
                x.Append(" (disposed)");
                return;
            }

            if (_pool.Count < 1)
            {
                x.Append(" (empty)");
                return;
            }

            try
            {
                var size = _pool.Count;
                var memory = size * TypeSize;

                x.Append("- ");
                x.Append(size);
                x.Append(" (");
                x.Append(Mirror.Utils.PrettyBytes(memory));
                x.Append(")");
            }
            catch (Exception ex)
            {
                x.Append("- ");
                x.Append(_pool.Count);
                x.Append(" (");
                x.Append(ex.Message);
                x.Append(")");
            }
        });
}