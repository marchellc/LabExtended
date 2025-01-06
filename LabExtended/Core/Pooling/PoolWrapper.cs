namespace LabExtended.Core.Pooling;

public class PoolWrapper<T> : PoolObject, IDisposable
{
    public T Value { get; }

    public PoolWrapper(T value)
        => Value = value;

    public void Dispose()
        => ReturnToPool<PoolWrapper<T>>();
}