using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts an long range on a parameter.
/// </summary>
public class LongRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public long? MinimumValue { get; private set; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public long? MaximumValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the LongRangeRestriction class.
    /// </summary>
    public LongRangeRestriction()
    {

    }

    /// <summary>
    /// Creates a new <see cref="LongRangeRestriction"/> instance.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    public LongRangeRestriction(long? minimumValue, long? maximumValue)
    {
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, 2, out var parts))
            return false;

        if (!long.TryParse(parts[0], out var minValue))
            return false;

        if (!long.TryParse(parts[1], out var maxValue))
            return false;

        MinimumValue = minValue;
        MaximumValue = maxValue;

        return true;
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not long value)
        {
            error = "Supplied argument is not a signed 64-bit integer.";
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
        => $"64-bit signed integer range restriction (Minimum Value: {MinimumValue?.ToString() ?? "Undefined"} | Maximum Value: {MaximumValue?.ToString() ?? "Undefined"})";
}