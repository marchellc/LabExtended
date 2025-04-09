using LabExtended.Commands.Parameters.Parsers.Wrappers;

using UnityEngine;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parsers <see cref="Vector2"/>
/// </summary>
public class Vector2ParameterParser : AxisWrapperParser<Vector2>
{
    /// <inheritdoc cref="AxisWrapperParser{T}.AxisNames"/>
    public override char[] AxisNames { get; } = ['x', 'y'];

    /// <inheritdoc cref="AxisWrapperParser{T}.ToAxis(System.Collections.Generic.Dictionary{char,float},out string,out T)"/>
    public override bool ToAxis(Dictionary<char, float> values, out string error, out Vector2 result)
    {
        error = null;
        
        result = new(values['x'], values['y']);
        return true;
    }
}