using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Parsing
{
    public class DateTimeParser : ICommandParser
    {
        public string Name => "Date";
        public string Description => "A date. Look at 'https://learn.microsoft.com/en-us/dotnet/api/system.datetime.parse?view=net-8.0#system-datetime-parse(system-string)' for a list of valid formats.";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            try
            {
                failureMessage = null;
                result = DateTime.Parse(value);
                return true;
            }
            catch (Exception ex)
            {
                failureMessage = $"Faield to parse date: {ex.Message}";
                result = null;
                return false;
            }
        }
    }
}