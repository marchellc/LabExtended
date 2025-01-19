using System.Reflection;
using LabExtended.API;
using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.Commands.Parsing
{
    public class NumericParser : Interfaces.ICommandParser
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
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; } 

        public NumericParser(Type numericType)
        {
            Type = numericType;
            Parser = Parsers[numericType];

            MaxValue = numericType.FindField("MaxValue")?.GetValue(null) ?? 0;
            MinValue = numericType.FindField("MinValue")?.GetValue(null) ?? 0;
            
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = numericType;

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