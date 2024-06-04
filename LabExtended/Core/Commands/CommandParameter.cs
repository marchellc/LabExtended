using LabExtended.Core.Commands.Parsing;

using System.Reflection;

namespace LabExtended.Core.Commands
{
    public struct CommandParameter
    {
        public IArgumentParser Parser { get; }

        public CommandParameterFlags Flags { get; }

        public Type Type { get; }
        public ParameterInfo Parameter { get; }

        public object DefaultValue { get; }

        public bool IsOptional => (Flags & CommandParameterFlags.Optional) != 0;
        public bool IsCatchAll => (Flags & CommandParameterFlags.CatchAll) != 0;
    }
}