using System.Text;

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
    /// Whether or not the value is being parsed.
    /// </summary>
    public bool IsValue { get; set; }

    /// <summary>
    /// Gets the builder of the dictionary key.
    /// </summary>
    public StringBuilder KeyBuilder { get; } = new();

    /// <summary>
    /// Gets the builder of the dictionary value.
    /// </summary>
    public StringBuilder ValueBuilder { get; } = new();
    
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

        IsValue = false;
        
        Values.Clear();
        KeyBuilder.Clear();
        ValueBuilder.Clear();
    }
}