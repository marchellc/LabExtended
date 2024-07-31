using LabExtended.Core.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Core.Commands.Parsing
{
    public class NumericParser : ICommandParser
    {
        public static IReadOnlyDictionary<Type, Func<string, object>> Parsers { get; } = new Dictionary<Type, Func<string, object>>()
        {
            [typeof(sbyte)] = str => sbyte.Parse(str),
            [typeof(byte)] = str => byte.Parse(str),

            [typeof(ushort)] = str => ushort.Parse(str),
            [typeof(short)] = str => short.Parse(str),

            [typeof(uint)] = str => uint.Parse(str),
            [typeof(int)] = str => int.Parse(str),

            [typeof(ulong)] = str => ulong.Parse(str),
            [typeof(long)] = str => long.Parse(str),

            [typeof(float)] = str => float.Parse(str),
            [typeof(decimal)] = str => decimal.Parse(str),
            [typeof(double)] = str => double.Parse(str)
        };

        public string Name => "A number.";
        public string Description => $"A number between {MinValue} and {MaxValue}.";

        public object MaxValue { get; }
        public object MinValue { get; }

        public Type Type { get; }

        public Func<string, object> Parser { get; }

        public NumericParser(Type numericType)
        {
            Type = numericType;
            Parser = Parsers[numericType];

            MaxValue = numericType.FindField("MaxValue")?.GetValue(null) ?? 0;
            MinValue = numericType.FindField("MinValue")?.GetValue(null) ?? 0;
        }

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            try
            {
                failureMessage = null;
                result = Parser(value);
                return true;
            }
            catch (Exception ex)
            {
                failureMessage = $"Failed to parse number: {ex.Message}";
                result = null;
                return false;
            }
        }
    }
}