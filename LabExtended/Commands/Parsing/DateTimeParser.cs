using LabExtended.Utilities;
using LabExtended.API;

using System.Reflection;

namespace LabExtended.Commands.Parsing
{
    public class DateTimeParser : Interfaces.ICommandParser
    {
        public string Name => "Date";
        public string Description => "A date. Look at 'https://learn.microsoft.com/en-us/dotnet/api/system.datetime.parse?view=net-8.0#system-datetime-parse(system-string)' for a list of valid formats.";

        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public DateTimeParser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(DateTime);

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
            try
            {
                failureMessage = null;
                result = DateTime.Parse(value);
                return true;
            }
            catch (Exception ex)
            {
                failureMessage = $"Failed to parse date: {ex.Message}";
                result = null;
                return false;
            }
        }
    }
}