using LabExtended.API.Pooling;

using UnityEngine;

namespace LabExtended.Commands.Parsing
{
    public class Vector3Parser : Interfaces.ICommandParser
    {
        public string Name => "3-axis vector";
        public string Description => "A 3-axis vector. Formatted as follows: 'numX numY numZ' (replace num with a real number)";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            var axis = DictionaryPool<char, float>.Rent();

            axis['x'] = 1f;
            axis['y'] = 1f;
            axis['z'] = 1f;

            if (!AxisParser.ParseAxis(value.Split(' '), axis, out var blackbox, out var error))
            {
                DictionaryPool<char, float>.Return(axis);

                failureMessage = $"Error: {error} | Blackbox: {blackbox}";
                return false;
            }

            result = new Vector3(axis['x'], axis['y'], axis['z']);

            DictionaryPool<char, float>.Return(axis);
            return true;
        }
    }
}