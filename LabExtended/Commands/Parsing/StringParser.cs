using LabExtended.API;
using LabExtended.Utilities;

using System.Reflection;

namespace LabExtended.Commands.Parsing
{
    public class StringParser : Interfaces.ICommandParser
    {
        public string Name => "A string of characters, or simply a word.";
        public string Description => "A string of characters, or simply a word.";
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public StringParser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(string);

            void Register(MethodInfo getter, string name)
            {
                if (getter is null || getter.IsStatic || dict.ContainsKey(name))
                    return;
                
                if (dict.ContainsKey(name))
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