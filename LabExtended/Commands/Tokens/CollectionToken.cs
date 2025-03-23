using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a collection token.
/// </summary>
public class CollectionToken : PoolObject, ICommandToken
{
    private CollectionToken() { }
    
    /// <summary>
    /// Gets or sets the character used to identify collections.
    /// </summary>
    public static char StartToken { get; set; } = '[';
    
    /// <summary>
    /// Gets or sets the character used to identify the end of a collection.
    /// </summary>
    public static char EndToken { get; set; } = ']';

    /// <summary>
    /// Gets or sets the character used to split items.
    /// </summary>
    public static char SplitToken { get; set; } = ',';
    
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
        => ObjectPool<CollectionToken>.Shared.Rent(null, () => new());

    /// <inheritdoc cref="ICommandToken.ReturnToken"/>
    public void ReturnToken()
        => ObjectPool<CollectionToken>.Shared.Return(this);

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();
        
        Value = string.Empty;
        Values.Clear();
    }
}