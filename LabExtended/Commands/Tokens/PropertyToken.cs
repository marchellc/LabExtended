﻿using LabExtended.Commands.Interfaces;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.Commands.Tokens;

/// <summary>
/// Represents a custom property token.
/// </summary>
public class PropertyToken : PoolObject, ICommandToken
{
    private PropertyToken() { }
    
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
    /// Gets an instance of the property token.
    /// </summary>
    public static PropertyToken Instance { get; } = new();
    
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the resolved value.
    /// </summary>
    public object? Value { get; set; }

    /// <inheritdoc cref="ICommandToken.NewToken"/>
    public ICommandToken NewToken()
        => ObjectPool<PropertyToken>.Shared.Rent(null, () => new());

    /// <inheritdoc cref="ICommandToken.ReturnToken"/>
    public void ReturnToken()
        => ObjectPool<PropertyToken>.Shared.Return(this);

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        Name = null;
        Value = null;
    }
}