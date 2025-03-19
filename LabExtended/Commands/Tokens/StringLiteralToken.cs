using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a parsed string literal.
/// </summary>
public class StringLiteralToken : ICommandToken
{
    /// <summary>
    /// Gets an instance of the <see cref="StringLiteralToken"/>.
    /// </summary>
    public static StringLiteralToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the value of the string literal.
    /// </summary>
    public PropertyToken Token { get; set; }
    
    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken() 
        => new StringLiteralToken();
}