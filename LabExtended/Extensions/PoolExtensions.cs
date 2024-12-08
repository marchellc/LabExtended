using NorthwoodLib.Pools;

using System.Text;

namespace LabExtended.Extensions
{
    public static class PoolExtensions
    {
        public static T[] ToArrayReturn<T>(this ListPool<T> pool, List<T> list)
        {
            var array = list.ToArray();

            pool.Return(list);
            return array;
        }

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
}