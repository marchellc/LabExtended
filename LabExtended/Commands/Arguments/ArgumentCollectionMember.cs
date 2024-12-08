namespace LabExtended.Commands.Arguments
{
    public class ArgumentCollectionMember
    {
        public ArgumentDefinition Definition { get; set; }
        public Func<object, object[], object> SetValue { get; set; }
    }
}