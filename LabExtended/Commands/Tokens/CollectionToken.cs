using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a collection token.
/// </summary>
public class CollectionToken : ICommandToken
{
    /// <summary>
    /// Gets an instance of <see cref="CollectionToken"/>.
    /// </summary>
    public static CollectionToken Instance { get; } = new();

    /// <summary>
    /// Gets or sets the value that is currently being parsed.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets a list of all parsed values.
    /// </summary>
    public List<string> Values { get; } = new();
    
    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => new CollectionToken();
}