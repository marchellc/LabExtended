using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a string word.
/// </summary>
public struct StringToken : ICommandToken
{
    /// <summary>
    /// Gets an instance of the string token.
    /// </summary>
    public static StringToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the token's value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new <see cref="StringToken"/>.
    /// </summary>
    public StringToken()
    {
        Value = string.Empty;
    }
    
    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken() 
        => new StringToken();
}