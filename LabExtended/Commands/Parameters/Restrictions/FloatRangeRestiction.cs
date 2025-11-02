using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts a float range on a parameter.
/// </summary>
public class FloatRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public float? MinimumValue { get; private set; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public float? MaximumValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the FloatRangeRestriction class.
    /// </summary>
    public FloatRangeRestriction()
    {

    }

    /// <summary>
    /// Creates a new <see cref="FloatRangeRestriction"/> instance.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    public FloatRangeRestriction(float? minimumValue, float? maximumValue)
    {
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, 2, out var parts))
            return false;

        if (!float.TryParse(parts[0], out var minValue))
            return false;

        if (!float.TryParse(parts[1], out var maxValue))
            return false;

        MinimumValue = minValue;
        MaximumValue = maxValue;

        return true;
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not float value)
        {
            error = "Supplied argument is not a floating point integer.";
            return false;
        }

        if (value < MinimumValue)
        {
            error = $"Value must be greater than {MinimumValue}";
            return false;
        }

        if (value > MaximumValue)
        {
            error = $"Value must be less than {MaximumValue}";
            return false;
        }
        
        error = null;
        return true;
    }
    
    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"floating point integer range restriction (Minimum Value: {MinimumValue?.ToString() ?? "Undefined"} | Maximum Value: {MaximumValue?.ToString() ?? "Undefined"})";
}