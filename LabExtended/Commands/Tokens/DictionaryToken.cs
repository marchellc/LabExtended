using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a dictionary token.
/// </summary>
public class DictionaryToken : PoolObject, ICommandToken
{
    private DictionaryToken() { }
    
    /// <summary>
    /// Gets or sets the starting token of a dictionary.
    /// </summary>
    public static char StartToken { get; set; } = '{';
    
    /// <summary>
    /// Gets or sets the ending token of a dictionary.
    /// </summary>
    public static char EndToken { get; set; } = '}';

    /// <summary>
    /// Gets or sets the pair splitter token of a dictionary.
    /// </summary>
    public static char SplitToken { get; set; } = ':';
    
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
        => ObjectPool<DictionaryToken>.Shared.Rent(null, () => new());

    /// <inheritdoc cref="ICommandToken.ReturnToken"/>
    public void ReturnToken()
        => ObjectPool<DictionaryToken>.Shared.Return(this);

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();

        CurKey = null;
        CurValue = null;
        
        Values.Clear();
    }
}