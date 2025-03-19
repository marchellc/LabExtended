using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a dictionary token.
/// </summary>
public class DictionaryToken : ICommandToken
{
    /// <summary>
    /// Gets or sets the starting token of a dictionary.
    /// </summary>
    public static char StartToken { get; set; } = '{';
    
    /// <summary>
    /// Gets or sets the ending token of a dictionary.
    /// </summary>
    public static char EndToken { get; set; } = '}';
    
    /// <summary>
    /// Gets an instance of <see cref="DictionaryToken"/>.
    /// </summary>
    public static DictionaryToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the current key.
    /// </summary>
    public string? CurKey { get; set; }
    
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public string? CurValue { get; set; }

    /// <summary>
    /// Gets the dictionary that contains parsed values.
    /// </summary>
    public Dictionary<string, string> Values { get; } = new();
    
    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => new DictionaryToken();
}