using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts a sbyte range on a parameter.
/// </summary>
public class SByteRangeRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public sbyte? MinimumValue { get; private set; }
    
    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public sbyte? MaximumValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the SByteRangeRestriction class.
    /// </summary>
    public SByteRangeRestriction()
    {

    }

    /// <summary>
    /// Creates a new <see cref="SByteRangeRestriction"/> instance.
    /// </summary>
    /// <param name="minimumValue"></param>
    /// <param name="maximumValue"></param>
    public SByteRangeRestriction(sbyte? minimumValue, sbyte? maximumValue)
    {
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
    }

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, 2, out var parts))
            return false;

        if (!sbyte.TryParse(parts[0], out var minValue))
            return false;

        if (!sbyte.TryParse(parts[1], out var maxValue))
            return false;

        MinimumValue = minValue;
        MaximumValue = maxValue;

        return true;
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not sbyte value)
        {
            error = "Supplied argument is not a short byte";
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
        => $"8-bit signed integer range restriction (Minimum Value: {MinimumValue?.ToString() ?? "Undefined"} | Maximum Value: {MaximumValue?.ToString() ?? "Undefined"})";
}