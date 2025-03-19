using LabExtended.Core.Pooling.Pools;
using LabExtended.Utilities;
using LabExtended.API;

using System.Reflection;

using UnityEngine;

namespace LabExtended.Commands.Parsing
{
    public class Vector2Parser : Interfaces.ICommandParser
    {
        public string Name => "2-axis vector";
        public string Description => "A 2-axis vector. Formatted as follows: 'numX numY' (replace num with a real number)";
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public Vector2Parser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(Vector2);

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
            failureMessage = null;
            result = null;

            var axis = DictionaryPool<char, float>.Shared.Rent(2);

            axis['x'] = 1f;
            axis['y'] = 1f;

            if (!AxisParser.ParseAxis(value.Split(' '), axis, out var error))
            {
                DictionaryPool<char, float>.Shared.Return(axis);

                failureMessage = $"Error: {error}";
                return false;
            }

            result = new Vector2(axis['x'], axis['y']);

            DictionaryPool<char, float>.Shared.Return(axis);
            return true;
        }
    }
}