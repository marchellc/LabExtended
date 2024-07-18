namespace LabExtended.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class OptionalAttribute : Attribute
    {
        public object DefaultValue { get; }

        public OptionalAttribute(object defaultValue = null)
            => DefaultValue = defaultValue;
    }
}