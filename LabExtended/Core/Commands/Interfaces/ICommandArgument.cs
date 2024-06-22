using System.Reflection;

namespace LabExtended.Core.Commands.Interfaces
{
    public interface ICommandArgument
    {
        string Name { get; set; }
        string Description { get; set; }

        Type Type { get; set; }

        bool IsOptional { get; set; }
        bool IsRemainder { get; set; }

        object DefaultValue { get; set; }

        ICommandParser Parser { get; set; }

        ParameterInfo Parameter { get; set; }
    }
}