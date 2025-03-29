using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters.Parsers;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts precision used in <see cref="PlayerParameterParser"/> and <see cref="PlayerListParameterParser"/>.
/// </summary>
public class PlayerPrecisionRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Gets the set precision.
    /// </summary>
    public double Precision { get; }
    
    /// <summary>
    /// Creates a new <see cref="PlayerPrecisionRestriction"/> instance.
    /// </summary>
    /// <param name="precision">The required precision.</param>
    public PlayerPrecisionRestriction(double precision)
        => Precision = precision;

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        error = null;
        return true;
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"Player Matching Precision Modifier ({Precision} [0 - 1])";
}