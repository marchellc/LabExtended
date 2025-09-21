using NorthwoodLib.Pools;

using System.Text;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions for Northwood's pooling library.
/// </summary>
public static class PoolExtensions
{
    /// <summary>
    /// Converts the list to an array, returns it to the pool and then returns the array.
    /// </summary>
    public static T[] ToArrayReturn<T>(this ListPool<T> pool, List<T> list)
    {
        var array = list.ToArray();

        pool.Return(list);
        return array;
    }

    /// <summary>
    /// Builds a string using a pooled string builder.
    /// </summary>
    public static string BuildString(this StringBuilderPool pool, Action<StringBuilder> builder)
    {
        if (pool is null)
            throw new ArgumentNullException(nameof(pool));

        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var instance = pool.Rent();

        builder(instance);
        return StringBuilderPool.Shared.ToStringReturn(instance);
    }
}