namespace LabExtended.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoaderInitializeAttribute : Attribute
    {
        public short Priority { get; }

        public LoaderInitializeAttribute(short priority)
            => Priority = priority;
    }
}