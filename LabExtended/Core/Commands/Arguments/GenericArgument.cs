using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Arguments
{
    public class GenericArgument<T> : CommandArgument
    {
        public GenericArgument(string name, string description, T defaultValue = default, ICommandParser parser = null)
            : base(name, description, typeof(T), defaultValue, parser)
        { }
    }
}