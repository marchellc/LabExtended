using System.Reflection;

namespace LabExtended.Core.Hooking
{
    public struct HookEventObject
    {
        public readonly string PropertyName;
        public readonly object PropertyValue;

        public readonly PropertyInfo PropertyRef;

        public HookEventObject(string propName, object propValue, PropertyInfo propertyRef)
        {
            PropertyName = propName;
            PropertyValue = propValue;
            PropertyRef = propertyRef;
        }
    }
}