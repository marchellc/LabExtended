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

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "A valid string is required.";
                return false;
            }

            var args = value.Split(' ');

            var xAxis = 0f;
            var yAxis = 0f;
            var zAxis = 0f;
            var wAxis = 0f;

            foreach (var arg in args)
            {
                if (arg.Length != 2)
                {
                    failureMessage = $"Encountered an invalid item: {arg}";
                    return false;
                }

                try
                {
                    var lastArg = arg[1];
                    var firstArg = arg[0];

                    var number = char.IsNumber(lastArg) ? float.Parse(lastArg.ToString()) : float.Parse(firstArg.ToString());
                    var axis = char.IsNumber(lastArg) ? firstArg : lastArg;

                    if (axis is 'x' || axis is 'X')
                        xAxis = number;
                    else if (axis is 'y' || axis is 'Y')
                        yAxis = number;
                    else if (axis is 'z' || axis is 'Z')
                        zAxis = number;
                    else if (axis is 'w' || axis is 'W')
                        wAxis = number;
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

            result = new Quaternion(xAxis, yAxis, zAxis, wAxis);
            return true;
        }
    }
}