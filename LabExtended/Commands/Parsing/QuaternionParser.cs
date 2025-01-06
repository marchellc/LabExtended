using LabExtended.Core.Pooling.Pools;

using UnityEngine;

namespace LabExtended.Commands.Parsing
{
    public class QuaternionParser : Interfaces.ICommandParser
    {
        public string Name => "4-axis quaternion";
        public string Description => "A 4-axis quaternion. Formatted as follows: 'numX numY numZ numW' (replace num with a real number)";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            var axis = DictionaryPool<char, float>.Shared.Rent(4);

            axis['x'] = 1f;
            axis['y'] = 1f;
            axis['z'] = 1f;
            axis['w'] = 1f;

            if (!AxisParser.ParseAxis(value.Split(' '), axis, out var error))
            {
                DictionaryPool<char, float>.Shared.Return(axis);

                failureMessage = $"Error: {error}";
                return false;
            }

            result = new Quaternion(axis['x'], axis['y'], axis['z'], axis['w']);

            DictionaryPool<char, float>.Shared.Return(axis);
            return true;
        }
    }
}