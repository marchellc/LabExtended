using System.Drawing;

using Color = UnityEngine.Color;

namespace LabExtended.Commands.Parsing
{
    public class ColorParser : Interfaces.ICommandParser
    {
        public string Name => "Color";
        public string Description => "Color can be specified by it's hex value (ex. #ff0000 for red) or by it's RGB values (ex. 1r 2g 3b - at least one of these has to be specified).";

        public bool TryParse(string value, out string failureMessage, out object result)
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

                    result = new Color(htmlColor.A, htmlColor.B, htmlColor.G);
                    return true;
                }
                else
                {
                    var args = value.Split(' ');

                    var rValue = 0f;
                    var gValue = 0f;
                    var bValue = 0f;

                    if (args.Length < 1)
                    {
                        failureMessage = "At least one value is required.";
                        return false;
                    }

                    foreach (var arg in args)
                    {
                        if (!arg.Any(char.IsNumber) || !arg.Any(char.IsLetter))
                        {
                            failureMessage = $"Encountered an invalid value ({arg})";
                            return false;
                        }

                        var number = float.Parse(new string(arg.Where(char.IsNumber).ToArray()));
                        var type = arg.First(char.IsLetter);

                        if (type is 'r' || type is 'R')
                            rValue = number;
                        else if (type is 'g' || type is 'G')
                            gValue = number;
                        else if (type is 'b' || type is 'B')
                            bValue = number;
                        else
                        {
                            failureMessage = $"Unknown color ({arg}): {type}";
                            return false;
                        }
                    }

                    result = new Color(rValue, gValue, bValue);
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