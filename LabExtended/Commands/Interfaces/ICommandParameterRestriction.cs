using LabExtended.Commands.Contexts;
using LabExtended.Commands.Parameters;

namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Represents the base interface for all command parameter arguments.
/// </summary>
public interface ICommandParameterRestriction
{
    /// <summary>
    /// Validates the value of a parsed argument.
    /// </summary>
    /// <param name="argument">The parsed argument value.</param>
    /// <param name="context">The parsing context.</param>
    /// <param name="parameter">The target parameter.</param>
    /// <param name="error">The error to show.</param>
    /// <returns>true if the argument is valid</returns>
    bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error);
}