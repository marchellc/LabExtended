namespace LabExtended.Core.Pooling;

public class PoolObject
{
    public object PoolInstance { get; internal set; }
    
    public bool IsPooled { get; internal set; }

    public void ReturnToPool<T>() where T : class
    {
        if (IsPooled || PoolInstance is null)
            return;

        if (PoolInstance is not PoolBase<T> poolBase)
            throw new Exception($"Invalid generic type: {typeof(T).FullName}");

        poolBase.Return((T)(object)this);
    }
    
    public virtual void OnConstructed() { }
    public virtual void OnReturned() { }
    public virtual void OnRented() { }
}