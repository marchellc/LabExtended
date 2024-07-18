using LabExtended.Commands.Arguments;

namespace LabExtended.Commands.Interfaces
{
    public interface ICommandValidator
    {
        void Validate(ArgumentDefinition definition, ref object argument);
    }
}