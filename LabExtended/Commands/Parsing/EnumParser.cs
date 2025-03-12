using HarmonyLib;

using LabExtended.API;
using LabExtended.API.CustomCommands.Formatting;

using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Values;

using System.Reflection;

namespace LabExtended.Commands.Parsing
{
    public class EnumParser : Interfaces.ICommandParser
    {
        public string Name => $"A fixed set of selectable values ({EnumType.Name})";
        public string Description => $"A fixed set of selectable values ({EnumType.Name}) (you can specify the value's name or numeric ID, use 'formatting enum {EnumType.Name}' to see a list of valid values).";

        public Type EnumType { get; }
        public Type UnderlyingType { get; }
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public EnumParser(Type enumType)
        {
            EnumType = enumType;
            UnderlyingType = Enum.GetUnderlyingType(enumType);

            if (!FormattingEnumCommand.EnumTypes.Contains(enumType))
                FormattingEnumCommand.EnumTypes.Add(enumType);
            
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var generic = typeof(EnumValue<>).MakeGenericType(enumType);

            void Register(MethodInfo getter, string name)
            {
                if (getter is null || getter.IsStatic)
                    return;
                
                var method = FastReflection.ForMethod(getter);
                
                dict.Add(name, player => method(player, Array.Empty<object>()));
            }
            
            foreach (var property in typeof(ExPlayer).GetProperties())
            {
                if (dict.ContainsKey(property.Name))
                    continue;

                if (property.PropertyType != enumType && property.PropertyType != generic)
                {
                    // support for containers
                    foreach (var insideProperty in property.PropertyType.GetProperties())
                    {
                        var name = insideProperty.Name;

                        if (dict.ContainsKey(name))
                            name = $"{property.Name}.{name}".ToLower();

                        if (insideProperty.PropertyType == enumType)
                        {
                            Register(insideProperty.GetGetMethod(false), name);
                        }
                        else if (insideProperty.PropertyType == generic)
                        {
                            var enumGetter = insideProperty.GetGetMethod(false);
                    
                            if (enumGetter is null)
                                continue;

                            var enumValue = FastReflection.ForMethod(enumGetter);
                            var valueProperty = generic.PropertyGetter("Value");
                    
                            if (valueProperty is null)
                                continue;

                            var valueGetter = FastReflection.ForMethod(valueProperty);
                    
                            dict.Add(name.ToLower(), player =>
                            {
                                var value = enumValue(player, Array.Empty<object>());
                                return valueGetter(value, Array.Empty<object>());
                            });
                        }
                    }
                }
                else if (property.PropertyType == enumType)
                {
                    Register(property.GetGetMethod(false), property.Name.ToLower());
                }
                else
                {
                    var enumGetter = property.GetGetMethod(false);
                    
                    if (enumGetter is null)
                        continue;

                    var enumValue = FastReflection.ForMethod(enumGetter);
                    var valueProperty = generic.PropertyGetter("Value");
                    
                    if (valueProperty is null)
                        continue;

                    var valueGetter = FastReflection.ForMethod(valueProperty);
                    
                    dict.Add(property.Name.ToLower(), player =>
                    {
                        var value = enumValue(player, Array.Empty<object>());
                        return valueGetter(value, Array.Empty<object>());
                    });
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
                failureMessage = $"String cannot be empty or white-spaced.";
                return false;
            }

            value = value.Replace(".", ",");

            if (value.TrySplit(',', true, null, out var parts) && parts.Length > 1 && !EnumType.IsBitwiseEnum())
            {
                failureMessage = "This enum does not support bitwise operations (only a singular value can be specified).";
                return false;
            }

            try
            {
                result = Enum.Parse(EnumType, value, true);
                return true;
            }
            catch (Exception ex)
            {
                if (!FormattingEnumCommand.EnumTypes.Contains(EnumType))
                    FormattingEnumCommand.EnumTypes.Add(EnumType);
                
                failureMessage = $"Value \"{value}\" could not be parsed into a valid \"{EnumType.Name}\" enum.\n" +
                                 $"You can view a list of valid values by using the \"formatting enum {EnumType.Name}\" command" +
                                 $" (or \"formatting enum {EnumType.FullName}\" if there are more enums with the same name)." +
                                 $"\nError: {ex.Message}";
                
                result = null;
                return false;
            }
        }
    }
}
