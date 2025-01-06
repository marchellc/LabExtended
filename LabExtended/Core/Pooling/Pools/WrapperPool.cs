using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

public class WrapperPool<T> : ObjectPool<PoolWrapper<T>>
{
    private static new volatile WrapperPool<T> _shared;

    public static new WrapperPool<T> Shared => _shared ??= new WrapperPool<T>();

    public PoolWrapper<T> Rent(Action<T> setup = null, Func<T> constructor = null)
        => base.Rent(wrapper => setup.InvokeSafe(wrapper.Value), () => new PoolWrapper<T>(constructor()));
}