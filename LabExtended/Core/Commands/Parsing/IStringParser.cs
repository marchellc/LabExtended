using Common.Results;

namespace LabExtended.Core.Commands.Parsing
{
    public interface IStringParser
    {
        IResult Parse(string line, CommandParameter[] commandParameters);
    }
}