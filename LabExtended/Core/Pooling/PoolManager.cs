using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Pooling;

public static class PoolManager
{
    private static volatile Dictionary<Type, List<Tuple<int, string, object>>> _pools = new Dictionary<Type, List<Tuple<int, string, object>>>();
    private static volatile int _idClock = 0;

    public static bool TryGetPool<T>(int id, out PoolBase<T> poolBase) where T : class
    {
        poolBase = null;

        if (!_pools.TryGetValue(typeof(T), out var pools))
            return false;

        if (!pools.TryGetFirst(x => x.Item1 == id, out var foundPool))
            return false;

        poolBase = (PoolBase<T>)foundPool.Item3;
        return true;
    }
    
    public static void Register<T>(PoolBase<T> poolBase) where T : class
    {
        if (poolBase is null)
            throw new ArgumentNullException(nameof(poolBase));

        if (!_pools.TryGetValue(typeof(T), out var pools))
            _pools[typeof(T)] = pools = new List<Tuple<int, string, object>>();

        if (pools.Any(x => x.Item1 == poolBase.Id || x.Item3 == poolBase))
            return;

        poolBase.Id = _idClock++;
        pools.Add(new Tuple<int, string, object>(poolBase.Id, poolBase.Name, poolBase));

        if (ApiLoader.ApiConfig.PoolSection.InitialSizes.TryGetValue(poolBase.Name, out var initSize))
            poolBase.Preload(initSize, () => Activator.CreateInstance<T>());

        ApiLog.Debug("Pools", $"Registered pool {poolBase.Name} (ID: &6{poolBase.Id}&r)");
    }

    public static void Unregister<T>(PoolBase<T> poolBase) where T : class
    {
        if (poolBase is null)
            throw new ArgumentNullException(nameof(poolBase));

        if (!_pools.TryGetValue(typeof(T), out var pools))
            return;

        if (pools.RemoveAll(x => x.Item3 == poolBase) < 1)
            return;
        
        ApiLog.Debug("Pools", $"Unregistered pool {poolBase.Name} (ID: &6{poolBase.Id}&r)");
    }
    
    private static void OnRoundWaiting()
    {
        if (_pools.Count < 1 || !_pools.Any(x => x.Value.Count > 0))
        {
            ApiLog.Debug("Pools", $"No pools were registered.");
            return;
        }
        
        ApiLog.Debug("Pools", StringBuilderPool.Shared.BuildString(x =>
        {
            x.Append(_pools.Sum(y => y.Value.Count));
            x.Append(" registered pool(s)");
            x.AppendLine();

            foreach (var pair in _pools)
            {
                foreach (var pool in pair.Value)
                {
                    x.Append(pool.Item3);
                    x.AppendLine();
                }
            }
        }));
    }

    [LoaderInitialize(1)]
    private static void Init()
    {
        InternalEvents.OnRoundWaiting += OnRoundWaiting;
    }
}