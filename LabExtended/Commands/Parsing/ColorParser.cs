using LabExtended.Core.Pooling.Pools;
using LabExtended.Utilities;
using LabExtended.API;

using System.Drawing;
using System.Reflection;

using Color = UnityEngine.Color;

namespace LabExtended.Commands.Parsing
{
    public class ColorParser : Interfaces.ICommandParser
    {
        public string Name => "Color";
        public string Description => "Color can be specified by it's hex value (ex. #ff0000 for red) or by it's RGB values (ex. 1r 2g 3b - at least one of these has to be specified).";

        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public ColorParser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(Color);

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
                failureMessage = "A valid string is required.";
                return false;
            }

            try
            {
                if (value.StartsWith("#"))
                {
                    var htmlColor = ColorTranslator.FromHtml(value);

                    result = new Color(htmlColor.A, htmlColor.B, htmlColor.G, htmlColor.A);
                    return true;
                }
                else
                {
                    var args = value.Split(' ');
                    var axis = DictionaryPool<char, float>.Shared.Rent();

                    axis['a'] = 0f;
                    axis['r'] = 0f;
                    axis['g'] = 0f;
                    axis['b'] = 0f;

                    if (!AxisParser.ParseAxis(args, axis, out failureMessage))
                    {
                        DictionaryPool<char, float>.Shared.Return(axis);
                        return false;
                    }

                    result = new Color(axis['r'], axis['g'], axis['b'], axis['a']);

                    DictionaryPool<char, float>.Shared.Return(axis);
                    return true;
                }
            }
            catch (Exception ex)
            {
                failureMessage = $"Failed while parsing color: {ex.Message}";
                return false;
            }
        }
    }
}