using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Arguments;

/// <summary>
/// Specifies the minimum and / or maximum length of a string.
/// </summary>
public class StringLengthRangeArgument : ICommandParameterArgument
{
    /// <summary>
    /// Gets the minimum length of a string.
    /// </summary>
    public int? MinimumLength { get; }
    
    /// <summary>
    /// Gets the maximum length of a string.
    /// </summary>
    public int? MaximumLength { get; }

    /// <summary>
    /// Creates a new <see cref="StringLengthRangeArgument"/> instance.
    /// </summary>
    /// <param name="minimumLength">The minimum length.</param>
    /// <param name="maximumLength">The maximum length.</param>
    public StringLengthRangeArgument(int? minimumLength, int? maximumLength)
    {
        MinimumLength = minimumLength;
        MaximumLength = maximumLength;
    }

    /// <inheritdoc cref="ICommandParameterArgument.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        error = null;
        
        if (argument is not string str)
            return true;

        if (MinimumLength.HasValue && str.Length < MinimumLength.Value)
        {
            error = $"Must be greater than {MinimumLength.Value} characters long.";
            return false;
        }

        if (MaximumLength.HasValue && str.Length > MaximumLength.Value)
        {
            error = $"Must be less than {MaximumLength.Value} characters long.";
            return false;
        }
        
        return true;
    }

    /// <inheritdoc cref="StringLengthRangeArgument"/>
    public override string ToString()
        => $"String Length Range (Minimum Length: {MinimumLength ?? -1} | Maximum Length: {MaximumLength ?? -1})";
}