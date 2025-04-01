namespace LabExtended.Commands.Interfaces;

/// <summary>
/// Represents a token inside a command argument.
/// </summary>
public interface ICommandToken
{
    /// <summary>
    /// Gets a new token instance of the same type.
    /// </summary>
    /// <returns>The new token instance.</returns>
    ICommandToken NewToken();

    /// <summary>
    /// Returns this token instance to the pool.
    /// </summary>
    void ReturnToken();
}