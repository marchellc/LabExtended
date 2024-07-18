using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Parsing
{
    public class BoolParser : ICommandParser
    {
        public static Dictionary<bool, List<string>> AcceptedValues { get; } = new Dictionary<bool, List<string>>()
        {
            [true] = new List<string>() { "true", "yes", "yeah", "yeh", "y", "1" },
            [false] = new List<string>() { "false", "no", "n", "nah", "n", "0" },
        };

        public string Name => "Boolean";
        public string Description => $"A boolean (negative/positive) value. \nAccepted POSITIVE values: {string.Join(",", AcceptedValues[true])}\nAccepted NEGATIVE values: {string.Join(",", AcceptedValues[false])}";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "Value must be a valid character.";
                return false;
            }

            foreach (var pair in AcceptedValues)
            {
                if (pair.Value.Any(s => s.ToLower() == value.ToLower()))
                {
                    result = pair.Key;
                    return true;
                }
            }

            failureMessage = $"Unknown value: {value}";
            return false;
        }
    }
}