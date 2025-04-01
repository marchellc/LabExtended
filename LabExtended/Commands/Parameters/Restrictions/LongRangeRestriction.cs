using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts an long range on a parameter.
/// </summary>
public class LongRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public long? MinimumValue { get; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public long? MaximumValue { get; }

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