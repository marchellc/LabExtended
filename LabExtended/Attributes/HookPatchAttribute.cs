namespace LabExtended.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HookPatchAttribute : Attribute
    {
        public Type EventType { get; }

        public bool IsFunctionPatch { get; }

        public HookPatchAttribute(Type eventType, bool isFunction = false)
        {
            if (eventType is null)
                throw new ArgumentNullException(nameof(eventType));

            EventType = eventType;
            IsFunctionPatch = isFunction;
        }
    }
}