using System.Globalization;

using LabExtended.Commands.Tokens;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Parsers.Wrappers;

/// <summary>
/// Parses axis-based values (like colors or vectors).
/// </summary>
public abstract class AxisWrapperParser<T> : CommandParameterParser
{
    /// <summary>
    /// Gets the name of axis.
    /// </summary>
    public abstract char[] AxisNames { get; }

    /// <summary>
    /// Converts parameters into a value.
    /// </summary>
    /// <param name="values">The parameters.</param>
    /// <param name="error">Conversion error.</param>
    /// <param name="result">Conversion result.</param>
    /// <returns>true if the conversion was a success</returns>
    public abstract bool ToAxis(Dictionary<char, float> values, out string error, out T result);

    /// <summary>
    /// Converts string into target value.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <param name="result">Parsed value.</param>
    /// <returns>true if the conversion was a success</returns>
    public virtual bool ToAxis(string value, out T result)
    {
        result = default;
        return false;
    }

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        var sourceString = string.Empty;

        if (token is PropertyToken propertyToken
            && propertyToken.TryGet<object>(context, null, out var result))
        {
            if (result.GetType() == parameter.Type.Type)
                return new(true, result, null, parameter, this);

            if (result.GetType() == typeof(string))
                sourceString = result as string;
            else
                return new(false, null, $"Unsupported property type: {result.GetType().FullName}", parameter, this);
        }

        if (sourceString == string.Empty)
        {
            if (token is StringToken stringToken)
                sourceString = stringToken.Value;
            else
                return new(false, null, $"Unsupported token: {token.GetType().Name}", parameter, this);
        }

        sourceString = sourceString.Trim();
        
        if (ToAxis(sourceString, out var preResult))
            return new(true, preResult, null, parameter, this);

        var axis = DictionaryPool<char, float>.Shared.Rent();
        var parts = sourceString.Split(CommandManager.spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
        
        if (sourceString.Contains(","))
            parts = parts.Concat(sourceString.Split(CommandManager.commaSeparator, StringSplitOptions.RemoveEmptyEntries)).ToArray();

        parts.TrimStrings();
        
        for (var i = 0; i < AxisNames.Length; i++)
            axis.Add(AxisNames[i], 0f);

        if (parts.Length == 1 && !parts[0].Any(x => char.IsLetter(x) && x != ',' && x != '.' && !char.IsNumber(x)))
        {
            var part = parts[0];
            
            if (!float.TryParse(part, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var axisValue))
            {
                DictionaryPool<char, float>.Shared.Return(axis);
                return new(false, null, $"Could not parse axis value", parameter, this);
            }

            for (var i = 0; i < AxisNames.Length; i++)
                axis[AxisNames[i]] = axisValue;

            if (!ToAxis(axis, out var convError, out var convResult))
            {
                DictionaryPool<char, float>.Shared.Return(axis);
                return new(false, null, convError, parameter, this);
            }

            DictionaryPool<char, float>.Shared.Return(axis);
            return new(true, convResult, null, parameter, this);
        }
        else
        {
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                
                char? axisName = null;

                if (part.TryGetFirst(x => char.IsLetter(x) && x != ',' && x != '.' && x != '-' && !char.IsNumber(x),
                        out var axisResult))
                {
                    axisName = axisResult;
                }
                else if (i < AxisNames.Length && i < axis.Count)
                {
                    axisName = AxisNames[i];
                }

                if (!axisName.HasValue)
                {
                    DictionaryPool<char, float>.Shared.Return(axis);
                    return new(false, null, $"Undefined axis name at position {i} (part: {part})", parameter, this);
                }

                if (!axis.ContainsKey(axisName.Value))
                {
                    DictionaryPool<char, float>.Shared.Return(axis);
                    return new(false, null, $"Invalid axis name at position {i} (part: {part})", parameter, this);
                }

                var numbers = part.Where(x => x != axisName.Value);
                var numbersStr = new string(numbers.ToArray());

                if (!float.TryParse(numbersStr, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var axisValue))
                {
                    DictionaryPool<char, float>.Shared.Return(axis);
                    return new(false, null, $"Could not parse axis value at position {i} (part: {part})", parameter, this);
                }

                axis[axisName.Value] = axisValue;
            }

            if (!ToAxis(axis, out var convError, out var convResult))
            {
                DictionaryPool<char, float>.Shared.Return(axis);
                return new(false, null, convError, parameter, this);
            }
            
            DictionaryPool<char, float>.Shared.Return(axis);
            return new(true, convResult, null, parameter, this);
        }
    }
}