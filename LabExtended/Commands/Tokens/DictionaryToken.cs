using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a dictionary token.
/// </summary>
public struct DictionaryToken : ICommandToken
{
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
    public Dictionary<string, string> Values { get; }

    /// <summary>
    /// Creates a new <see cref="DictionaryToken"/> instance.
    /// </summary>
    public DictionaryToken()
    {
        Values = new();
    }
    
    public ICommandToken NewToken()
        => new DictionaryToken();
}