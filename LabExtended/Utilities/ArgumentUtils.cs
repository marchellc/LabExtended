namespace LabExtended.Utilities
{
    public static class ArgumentUtils
    {
        public static bool TryGet<T>(Func<T> getter, out T value) where T : class
        {
            value = getter();
            return value != null;
        }

        public static void Null(object obj, string name)
        {
            if (obj is null)
                throw new ArgumentNullException(name);
        }
    }
}