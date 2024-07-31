using NorthwoodLib.Pools;

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
    }
}