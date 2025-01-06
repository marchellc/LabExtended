namespace LabExtended.Core.Pooling.Pools;

public class ObjectPool<T> : PoolBase<T>
    where T : PoolObject
{
    private static volatile ObjectPool<T> _shared;

    public static ObjectPool<T> Shared => _shared ??= new ObjectPool<T>();

    public override string Name { get; } = $"ObjectPool<{typeof(T).Name}>";

    public override void HandleRent(T item)
    {
        base.HandleRent(item);
        
        item.IsPooled = false;
        item.OnRented();
    }

    public override void HandleReturn(T item)
    {
        base.HandleReturn(item);
        
        item.IsPooled = true;
        item.OnReturned();
    }

    public override void HandleNewItem(T item)
    {
        base.HandleNewItem(item);
        
        item.PoolInstance = this;
        item.OnConstructed();
    }
}