using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Parsing
{
    public class TimeSpanParser : ICommandParser
    {
        public string Name => "Duration";
        public string Description => "Duration (specifies a span of time. Use 'formatting duration' to learn more).";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            result = null;
            failureMessage = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "String cannot be a white-space character.";
                return false;
            }

            var args = value.Split(' ');
            var time = (long)0;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].Trim();

                if (arg.Length <= 2 || !char.IsLetter(arg.Last()))
                {
                    failureMessage = $"Failed to parse '{arg}'!";
                    return false;
                }

                try
                {
                    var number = long.Parse(new string(arg.Where(a => !char.IsLetter(a)).ToArray()));
                    var unit = arg.Last();

                    if (unit is 'm')
                        number = TimeSpan.FromMinutes(number).Ticks;
                    else if (unit is 's' || unit is 'S')
                        number = TimeSpan.FromSeconds(number).Ticks;
                    else if (unit is 'h' || unit is 'H')
                        number = TimeSpan.FromHours(number).Ticks;
                    else if (unit is 'd' || unit is 'D')
                        number = TimeSpan.FromDays(number).Ticks;
                    else if (unit is 'M')
                        number = TimeSpan.FromDays(number * 30).Ticks;
                    else if (unit is 'y' || unit is 'Y')
                        number = TimeSpan.FromDays(number * 365).Ticks;
                    else
                    {
                        failureMessage = $"Unit '{unit}' is not a valid time unit.";
                        return false;
                    }

                    time += TimeSpan.FromTicks(number).Ticks;
                }
                catch (Exception ex)
                {
                    failureMessage = $"Duration parsing failed ({arg}): {ex.Message}";
                    return false;
                }
            }

            result = TimeSpan.FromTicks(time);
            return true;
        }
    }
}