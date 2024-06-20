using LabExtended.Core.Hooking.Enums;

using System.Reflection;

namespace LabExtended.Core.Hooking.Objects
{
    public struct HookDelegateObject
    {
        public readonly EventInfo Event;
        public readonly FieldInfo Field;

        public readonly object TypeInstance;

        public readonly HookPriority Priority;

        public HookDelegateObject(EventInfo eventInfo, FieldInfo fieldInfo, object typeInstance, HookPriority priority)
        {
            Event = eventInfo;
            Field = fieldInfo;
            Priority = priority;
            TypeInstance = typeInstance;
        }
    }
}