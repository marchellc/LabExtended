using LabExtended.Extensions;

namespace LabExtended.Commands.Parsing
{
    public static class AxisParser
    {
        public static readonly char[] IndexToAxis = new char[] { 'x', 'y', 'z', 'w' };

        public static bool ParseAxis(string[] parts, Dictionary<char, float>? axis, out string error)
        {
            error = string.Empty;

            if (parts.Length == 1 && !parts[0].Any(x => char.IsLetter(x) && x != ',' && x != '.' && !char.IsNumber(x)))
            {
                var part = parts[0];

                if (!float.TryParse(part, out var axisValue))
                {
                    error = $"Failed to parse axis value!";
                    return false;
                }

                foreach (var key in axis.Keys.ToList())
                    axis[key] = axisValue;

                return true;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (part?.Length < 2)
                    continue;

                char? axisName = null;

                if (part.TryGetFirst(x => char.IsLetter(x) && x != ',' && x != '.' && !char.IsNumber(x), out var axisResult))
                    axisName = axisResult;
                else if (i < IndexToAxis.Length && i < axis.Count)
                    axisName = IndexToAxis[i];

                if (!axisName.HasValue)
                {
                    error = $"Undefined axis name on index {i} (part: {part})";
                    return false;
                }

                if (!axis.ContainsKey(axisName.Value))
                {
                    error = $"Unknown axis name on index {i} (part: {part}, name: {axisName.Value})";
                    return false;
                }

                var numbers = part.Where(x => x != axisName.Value);
                var numbersStr = new string(numbers.ToArray());

                if (!float.TryParse(numbersStr, out var axisValue))
                {
                    error = $"Failed to parse value of axis {axisName.Value}!";
                    return false;
                }

                axis[axisName.Value] = axisValue;
            }

            return true;
        }
    }
}