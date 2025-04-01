namespace LabExtended.Commands.Utilities;

using Interfaces;

/// <summary>
/// Extensions targeting <see cref="ICommandToken"/>
/// </summary>
public static class CommandTokenExtensions
{
    /// <summary>
    /// Gets a new token instance of the specific type.
    /// </summary>
    /// <param name="commandToken">The original token.</param>
    /// <typeparam name="T">The token type.</typeparam>
    /// <returns>The retrieved token instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T NewToken<T>(this ICommandToken commandToken) where T : ICommandToken
    {
        if (commandToken is null)
            throw new ArgumentNullException(nameof(commandToken));

        return (T)commandToken.NewToken();
    }
}