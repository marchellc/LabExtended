using LabExtended.Core.Commands.Interfaces;

using UnityEngine;

namespace LabExtended.Core.Commands.Parsing
{
    public class Vector3Parser : ICommandParser
    {
        public string Name => "3-axis vector";
        public string Description => "A 3-axis vector. Formatted as follows: 'numX numY numZ' (replace num with a real number)";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "A valid string is required.";
                return false;
            }

            var args = value.Split(' ');

            var xAxis = 0f;
            var yAxis = 0f;
            var zAxis = 0f;

            foreach (var arg in args)
            {
                if (arg.Length < 2)
                {
                    failureMessage = $"Encountered an invalid item: {arg}";
                    return false;
                }

                try
                {
                    var number = float.Parse(new string(arg.Where(a => char.IsNumber(a)).ToArray()));
                    var axis = arg.Last();

                    if (axis is 'x' || axis is 'X')
                        xAxis = number;
                    else if (axis is 'y' || axis is 'Y')
                        yAxis = number;
                    else if (axis is 'z' || axis is 'Z')
                        zAxis = number;
                    else
                    {
                        failureMessage = $"Unknown axis: {axis} ({arg})";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    failureMessage = $"Failed to parse axis value '{arg}': {ex.Message}";
                    return false;
                }
            }

            result = new Vector3(xAxis, yAxis, zAxis);
            return true;
        }
    }
}