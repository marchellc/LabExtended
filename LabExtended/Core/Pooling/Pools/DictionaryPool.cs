using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

public class DictionaryPool<TKey, TValue> : PoolBase<Dictionary<TKey, TValue>>
{
    private static volatile DictionaryPool<TKey, TValue> _shared;

    public static DictionaryPool<TKey, TValue> Shared => _shared ??= new DictionaryPool<TKey, TValue>();

    public override string Name { get; } = $"DictionaryPool<{typeof(TKey).Name}, {typeof(TValue).Name}>";

    public Dictionary<TKey, TValue> Rent()
        => base.Rent(null, () => new Dictionary<TKey, TValue>());

    public Dictionary<TKey, TValue> Rent(int capacity)
        => base.Rent(null, () => new Dictionary<TKey, TValue>(capacity));

    public Dictionary<TKey, TValue> Rent(IDictionary<TKey, TValue> dictionary)
        => base.Rent(x => x.AddRange(dictionary), () => new Dictionary<TKey, TValue>(dictionary));

    public void Return(Dictionary<TKey, TValue> dictionary)
        => base.Return(dictionary, x => x.Clear());
}