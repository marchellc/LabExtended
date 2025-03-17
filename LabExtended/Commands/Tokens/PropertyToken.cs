using System.Reflection;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a custom property token.
/// </summary>
public struct PropertyToken : ICommandToken
{
    /// <summary>
    /// Gets an instance of the property token.
    /// </summary>
    public static PropertyToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the key of the property.
    /// </summary>
    public string Key { get; set; } 
    
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new property token.
    /// </summary>
    public PropertyToken()
    {
        Key = string.Empty;
        Name = string.Empty;
    }

    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => new PropertyToken();
}