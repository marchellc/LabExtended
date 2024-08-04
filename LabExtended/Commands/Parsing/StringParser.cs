namespace LabExtended.Commands.Parsing
{
    public class StringParser : Interfaces.ICommandParser
    {
        public string Name => "A string of characters, or simply a word.";
        public string Description => "A string of characters, or simply a word.";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = null;
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            result = value.Trim();
            failureMessage = null;
            return true;
        }
    }
}