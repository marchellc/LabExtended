using LabExtended.API;
using LabExtended.Utilities;

using System.Reflection;

namespace LabExtended.Commands.Parsing
{
    public class TimeSpanParser : Interfaces.ICommandParser
    {
        public string Name => "Duration";
        public string Description => "Duration (specifies a span of time. Use 'formatting duration' to learn more).";

        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }
        
        public TimeSpanParser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(TimeSpan);

            void Register(MethodInfo getter, string name)
            {
                if (getter is null || getter.IsStatic || dict.ContainsKey(name))
                    return;
                
                var method = FastReflection.ForMethod(getter);
                
                dict.Add(name, player => method(player, Array.Empty<object>()));
            }
            
            foreach (var property in typeof(ExPlayer).GetProperties())
            {
                if (dict.ContainsKey(property.Name))
                    continue;

                if (property.PropertyType != target)
                {
                    // support for containers
                    foreach (var insideProperty in property.PropertyType.GetProperties())
                    {
                        var name = insideProperty.Name;

                        if (dict.ContainsKey(name))
                            name = $"{property.Name}.{name}".ToLower();
                        
                        if (insideProperty.PropertyType == target)
                            Register(insideProperty.GetGetMethod(false), name);
                    }
                }
                else
                {
                    Register(property.GetGetMethod(false), property.Name.ToLower());
                }
            }

            PlayerProperties = dict;
        }
        
        public bool TryParse(ExPlayer sender, string value, out string failureMessage, out object result)
        {
            result = null;
            failureMessage = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "String cannot be a white-space character.";
                return false;
            }

            value = value.Replace(".", ",");

            var args = value.Split(',');
            var time = (long)0;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].Trim();

                if (arg.Length < 2 || !char.IsLetter(arg.Last()))
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
                    failureMessage = $"TimeSpan parsing failed ({arg}): {ex.Message}";
                    return false;
                }
            }

            result = TimeSpan.FromTicks(time);
            return true;
        }
    }
}