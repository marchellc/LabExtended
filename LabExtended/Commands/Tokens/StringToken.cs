using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a string word.
/// </summary>
public class StringToken : ICommandToken
{
    /// <summary>
    /// Gets or sets the character used to identify a full string token.
    /// </summary>
    public static char Token { get; set; } = '\"';
    
    /// <summary>
    /// Gets an instance of the string token.
    /// </summary>
    public static StringToken Instance { get; } = new();

    /// <summary>
    /// Gets or sets the token's value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
    
    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken() 
        => new StringToken();
}