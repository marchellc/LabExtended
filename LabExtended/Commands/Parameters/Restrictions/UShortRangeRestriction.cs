using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts a ushort range on a parameter.
/// </summary>
public class UShortRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public ushort? MinimumValue { get; private set; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public ushort? MaximumValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the UShortRangeRestriction class with no minimum or maximum value restrictions.
    /// </summary>
    public UShortRangeRestriction() : this(null, null)
    {

    }

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

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, 2, out var parts))
            return false;

        if (!ushort.TryParse(parts[0], out var minValue))
            return false;

        if (!ushort.TryParse(parts[1], out var maxValue))
            return false;

        MinimumValue = minValue;
        MaximumValue = maxValue;

        return true;
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