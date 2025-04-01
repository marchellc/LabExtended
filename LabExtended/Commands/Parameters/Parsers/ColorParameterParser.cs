using System.Drawing;

using LabExtended.Commands.Parameters.Parsers.Wrappers;

using Color = UnityEngine.Color;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parses <see cref="UnityEngine.Color"/>.
/// </summary>
public class ColorParameterParser : AxisWrapperParser<Color>
{
    /// <inheritdoc cref="AxisWrapperParser{T}.AxisNames"/>
    public override char[] AxisNames { get; } = ['r', 'g', 'b', 'a'];

    /// <inheritdoc cref="AxisWrapperParser{T}.ToAxis(string,out T)"/>
    public override bool ToAxis(string value, out Color result)
    {
        if (value.StartsWith("#"))
        {
            try
            {
                var systemColor = ColorTranslator.FromHtml(value);
                
                result = new(systemColor.R, systemColor.G, systemColor.B, systemColor.A);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }

            return true;
        }
        
        result = default;
        return false;
    }

    /// <inheritdoc cref="AxisWrapperParser{T}.ToAxis(System.Collections.Generic.Dictionary{char,float},out string,out T)"/>
    public override bool ToAxis(Dictionary<char, float> values, out string error, out Color result)
    {
        error = null;
        
        result = new(values['r'], values['g'], values['b'], values['a']);
        return true;
    }
}