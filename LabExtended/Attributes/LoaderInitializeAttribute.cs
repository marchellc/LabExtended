namespace LabExtended.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoaderInitializeAttribute : Attribute
    {
        public ushort Priority { get; }

        public LoaderInitializeAttribute(ushort priority)
            => Priority = priority;
    }
}