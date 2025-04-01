using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts an uint range on a parameter.
/// </summary>
public class UIntRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public uint? MinimumValue { get; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public uint? MaximumValue { get; }

    /// <summary>
    /// Creates a new <see cref="UIntRangeRestriction"/> instance.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    public UIntRangeRestriction(uint? minimumValue, uint? maximumValue)
    {
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }
    
    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not uint value)
        {
            error = "Supplied argument is not an unsigned 32-bit integer.";
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
        => $"32-bit unsigned integer range restriction (Minimum Value: {MinimumValue?.ToString() ?? "Undefined"} | Maximum Value: {MaximumValue?.ToString() ?? "Undefined"})";
}