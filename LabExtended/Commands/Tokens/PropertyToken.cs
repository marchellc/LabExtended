using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a custom property token.
/// </summary>
public class PropertyToken : PoolObject, ICommandToken
{
    private PropertyToken() { }
    
    /// <summary>
    /// Gets all registered properties.
    /// </summary>
    public static Dictionary<string, KeyValuePair<Type, Func<CommandContext, object>>> Properties { get; } = new();
    
    /// <summary>
    /// Gets or sets the character used to identify property command tokens.
    /// </summary>
    public static char StartToken { get; set; } = '$';

    /// <summary>
    /// Gets or sets the property bracket start character.
    /// </summary>
    public static char BracketStartToken { get; set; } = '{';
    
    /// <summary>
    /// Gets or sets the property bracket end character.
    /// </summary>
    public static char BracketEndToken { get; set; } = '}';
    
    /// <summary>
    /// Gets or sets the character used to split a property key and name.
    /// </summary>
    public static char SplitToken { get; set; } = '.';
    
    /// <summary>
    /// Gets an instance of the property token.
    /// </summary>
    public static PropertyToken Instance { get; } = new();

    /// <summary>
    /// Registers a new property.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="name">The property name.</param>
    /// <param name="property">The property getter.</param>
    public static void RegisterProperty<T>(string key, string name, Func<CommandContext, object> property)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        var fullKey = $"{key.ToLowerInvariant()}.{name.ToLowerInvariant()}";
        
        if (Properties.ContainsKey(fullKey))
            throw new Exception($"Property {fullKey} already registered");
        
        Properties.Add(fullKey, new(typeof(T), property));
    }
    
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
        => ObjectPool<PropertyToken>.Shared.Rent(null, () => new());

    /// <inheritdoc cref="ICommandToken.ReturnToken"/>
    public void ReturnToken()
        => ObjectPool<PropertyToken>.Shared.Return(this);

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();
        
        Key = string.Empty;
        Name = string.Empty;
    }
}