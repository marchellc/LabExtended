using LabExtended.Core.Hooking.Enums;

namespace LabExtended.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public class HookDescriptorAttribute : Attribute
    {
        public Type EventOverride { get; }

        public HookPriority Priority { get; }

        public HookDescriptorAttribute(HookPriority priority = HookPriority.Normal, Type eventOverride = null)
        {
            Priority = priority;
            EventOverride = eventOverride;
        }
    }
}