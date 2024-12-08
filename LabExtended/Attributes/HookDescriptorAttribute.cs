using LabExtended.Core.Hooking.Enums;

namespace LabExtended.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public class HookDescriptorAttribute : Attribute
    {
        public Type EventOverride { get; set; }

        public HookPriority Priority { get; set; } = HookPriority.Normal;

        public bool UseReflection { get; set; } = false;

        public HookDescriptorAttribute(HookPriority priority = HookPriority.Normal, Type eventOverride = null, bool useReflection = false)
        {
            Priority = priority;
            EventOverride = eventOverride;
            UseReflection = useReflection;
        }
    }
}