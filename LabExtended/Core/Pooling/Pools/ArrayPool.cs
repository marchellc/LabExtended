using LabExtended.Extensions;

namespace LabExtended.Core.Pooling.Pools;

public class ArrayPool<T>
{
    private static volatile ArrayPool<T> _shared;

    public static ArrayPool<T> Shared => _shared ??= new ArrayPool<T>();

    private Dictionary<int, List<T[]>> _pool = new Dictionary<int, List<T[]>>();

    public T[] Rent(int size, bool clearArray = false)
    {
        if (size < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        
        if (!_pool.TryGetValue(size, out var list))
            _pool[size] = list = new List<T[]>();

        if (list.Count < 1)
            return new T[size];

        var array = list.RemoveAndTake(0);

        if (clearArray)
            Array.Clear(array, 0, array.Length);

        return array;
    }

    public void Return(T[] array, bool cleanArray = true)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        var size = array.Length;

        if (cleanArray)
            Array.Clear(array, 0, size);

        if (_pool.TryGetValue(size, out var list))
            list.Add(array);
        else
            _pool[size] = new List<T[]>() { array };
    }
}