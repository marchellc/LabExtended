using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a custom property token.
/// </summary>
public class PropertyToken : ICommandToken
{
    /// <summary>
    /// Gets all registered properties.
    /// </summary>
    public static Dictionary<string, KeyValuePair<Type, Func<CommandContext, object>>> Properties { get; } = new();
    
    /// <summary>
    /// Gets or sets the character used to identify property command tokens.
    /// </summary>
    public static char Token { get; set; } = '$';
    
    /// <summary>
    /// Gets an instance of the property token.
    /// </summary>
    public static PropertyToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the key of the property.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => new PropertyToken();
}