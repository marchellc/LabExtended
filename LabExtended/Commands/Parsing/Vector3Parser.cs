using LabExtended.Core.Pooling.Pools;
using LabExtended.Utilities;
using LabExtended.API;

using System.Reflection;

using UnityEngine;

namespace LabExtended.Commands.Parsing
{
    public class Vector3Parser : Interfaces.ICommandParser
    {
        public string Name => "3-axis vector";
        public string Description => "A 3-axis vector. Formatted as follows: 'numX numY numZ' (replace num with a real number)";
        
        public Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        public Vector3Parser()
        {
            var dict = new Dictionary<string, Func<ExPlayer, object>>();
            var target = typeof(Vector3);

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
            failureMessage = null;
            result = null;

            var axis = DictionaryPool<char, float>.Shared.Rent(3);

            axis['x'] = 1f;
            axis['y'] = 1f;
            axis['z'] = 1f;

            if (!AxisParser.ParseAxis(value.Split(' '), axis, out var error))
            {
                DictionaryPool<char, float>.Shared.Return(axis);

                failureMessage = $"Error: {error}";
                return false;
            }

            result = new Vector3(axis['x'], axis['y'], axis['z']);

            DictionaryPool<char, float>.Shared.Return(axis);
            return true;
        }
    }
}