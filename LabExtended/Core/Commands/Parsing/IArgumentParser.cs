using Common.Results;

namespace LabExtended.Core.Commands.Parsing
{
    public interface IArgumentParser
    {
        uint? SpacedPartsCount { get; }

        NamedArgument[] NamedArguments { get; }

        IResult Parse(string arg);
    }
}