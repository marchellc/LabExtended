using LabExtended.Core.Commands.Interfaces;

using Utils.NonAllocLINQ;

using UnityEngine;

namespace LabExtended.Core.Commands.Parsing
{
    public class Vector2Parser : ICommandParser
    {
        public string Name => "2-axis vector";
        public string Description => "A 2-axis vector. Formatted as follows: 'numX numY' (replace num with a real number)";

        public bool TryParse(string value, out string failureMessage, out object result)
        {
            failureMessage = null;
            result = null;

            if (string.IsNullOrWhiteSpace(value))
            {
                failureMessage = "A valid string is required.";
                return false;
            }

            var args = value.Split(' ').ToList();

            value.Split(',').ForEach(s => args.AddIfNotContains(s));

            var xAxis = 0f;
            var yAxis = 0f;

            foreach (var arg in args)
            {
                if (arg.Length < 2)
                {
                    failureMessage = $"Encountered an invalid item: {arg}";
                    return false;
                }

                var number = float.Parse(new string(arg.Where(a => char.IsNumber(a))))
            }

            result = new Vector2(xAxis, yAxis);
            return true;
        }
    }
}