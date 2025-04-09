using LabExtended.Commands.Parameters.Parsers.Wrappers;

using UnityEngine;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parsers <see cref="Quaternion"/>
/// </summary>
public class QuaternionParameterParser : AxisWrapperParser<Quaternion>
{
    /// <inheritdoc cref="AxisWrapperParser{T}.AxisNames"/>
    public override char[] AxisNames { get; } = ['x', 'y', 'z', 'w'];

    /// <inheritdoc cref="AxisWrapperParser{T}.ToAxis(System.Collections.Generic.Dictionary{char,float},out string,out T)"/>
    public override bool ToAxis(Dictionary<char, float> values, out string error, out Quaternion result)
    {
        error = null;
        
        result = new(values['x'], values['y'], values['z'], values['w']);
        return true;
    }
}