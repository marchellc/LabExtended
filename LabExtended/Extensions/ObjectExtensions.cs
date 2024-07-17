namespace LabExtended.Extensions
{
    public static class ObjectExtensions
    {
        public static bool Is<T>(this object instance)
            => instance != null && instance is T;

        public static bool Is<T>(this object instance, out T result)
        {
            result = default;

            if (instance is null || instance is not T cast)
                return false;

            result = cast;
            return true;
        }

        public static bool IsEqualTo(this object instance, object otherInstance, bool countNull = false)
        {
            if (instance is null && otherInstance is null)
                return countNull;

            if ((instance is null && otherInstance != null) || (instance != null && otherInstance is null))
                return false;

            return instance == otherInstance;
        }
    }
}