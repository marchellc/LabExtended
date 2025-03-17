using System.Reflection;

using LabExtended.API;
using LabExtended.Utilities;

namespace LabExtended.Commands.Parsing
{
    public class BoolParser : Interfaces.ICommandParser
    {
        public static Dictionary<bool, List<string>> AcceptedValues { get; } = new Dictionary<bool, List<string>>()
        {
            [true] = new List<string>() { "true", "yes", "yeah", "yeh", "y", "1" },
            [false] = new List<string>() { "false", "no", "n", "nah", "n", "0" },
        };
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public string Name => "Boolean";
        public string Description => $"A boolean (negative/positive) value. \nAccepted POSITIVE values: {string.Join(",", AcceptedValues[true])}\nAccepted NEGATIVE values: {string.Join(",", AcceptedValues[false])}";

        public BoolParser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(bool);

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