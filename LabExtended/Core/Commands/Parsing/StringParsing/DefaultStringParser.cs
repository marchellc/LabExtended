using Common.Extensions;
using Common.Pooling.Pools;
using Common.Results;

namespace LabExtended.Core.Commands.Parsing.StringParsing
{
    public class DefaultStringParser //: IStringParser
    {
        public static List<char> Delimiters { get; } = new List<char>()
        {
            '\'',
            '"',
        };
    }
}