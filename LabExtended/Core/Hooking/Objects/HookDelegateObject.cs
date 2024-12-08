using HarmonyLib;

using LabExtended.Core.Hooking.Enums;
using LabExtended.Extensions;
using LabExtended.Utilities;

using System.Reflection;

namespace LabExtended.Core.Hooking.Objects
{
    public struct HookDelegateObject
    {
        public readonly EventInfo Event;
        public readonly FieldInfo Field;
        public readonly Func<object, object[], object> Invoker;
        public readonly AccessTools.FieldRef<object, Delegate> FieldGetter;

        public readonly object TypeInstance;

        public readonly HookPriority Priority;

        public HookDelegateObject(EventInfo eventInfo, FieldInfo fieldInfo, object typeInstance, HookPriority priority)
        {
            Event = eventInfo;
            Field = fieldInfo;
            Priority = priority;
            TypeInstance = typeInstance;

            Invoker = FastReflection.ForDelegate(eventInfo.EventHandlerType, eventInfo.EventHandlerType.FindMethod("Invoke"));
            FieldGetter = AccessTools.FieldRefAccess<Delegate>(fieldInfo.DeclaringType, fieldInfo.Name);
        }
    }
}