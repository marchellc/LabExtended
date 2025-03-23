using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a string word.
/// </summary>
public class StringToken : PoolObject, ICommandToken
{
    private StringToken() { }
    
    /// <summary>
    /// Gets or sets the character used to identify a full string token.
    /// </summary>
    public static char Token { get; set; } = '\"';
    
    /// <summary>
    /// Gets an instance of the string token.
    /// </summary>
    public static StringToken Instance { get; } = new();

    /// <summary>
    /// Gets or sets the token's value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => ObjectPool<StringToken>.Shared.Rent(null, () => new());
    
    /// <inheritdoc cref="ICommandToken.ReturnToken"/>
    public void ReturnToken()
        => ObjectPool<StringToken>.Shared.Return(this);

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();
        
        Value = string.Empty;
    }
}