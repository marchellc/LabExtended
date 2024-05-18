using System.Reflection;

namespace LabExtended.Core.Hooking
{
    public struct HookDelegate
    {
        public readonly EventInfo Event;
        public readonly FieldInfo Field;

        public HookDelegate(EventInfo eventInfo, FieldInfo fieldInfo)
        {
            Event = eventInfo;
            Field = fieldInfo;
        }
    }
}