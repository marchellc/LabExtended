namespace LabExtended.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class EventPatchAttribute : Attribute
{
    public Type EventType { get; }

    public bool IsFunctionPatch { get; }

    public EventPatchAttribute(Type eventType, bool isFunction = false)
    {
        if (eventType is null)
            throw new ArgumentNullException(nameof(eventType));

        EventType = eventType;
        IsFunctionPatch = isFunction;
    }
}