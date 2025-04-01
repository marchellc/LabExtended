using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts a ushort range on a parameter.
/// </summary>
public class UShortRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public ushort? MinimumValue { get; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public ushort? MaximumValue { get; }

    /// <summary>
    /// Creates a new <see cref="UShortRangeRestriction"/> instance.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    public UShortRangeRestriction(ushort? minimumValue, ushort? maximumValue)
    {
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }
    
    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not ushort value)
        {
            error = "Supplied argument is not a unsigned 16-bit integer.";
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
        => $"16-bit unsigned integer range restriction (Minimum Value: {MinimumValue?.ToString() ?? "Undefined"} | Maximum Value: {MaximumValue?.ToString() ?? "Undefined"})";
}