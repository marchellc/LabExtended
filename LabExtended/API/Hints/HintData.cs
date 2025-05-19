using LabExtended.Core.Pooling;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Hints;

/// <summary>
/// Represents parsed hint message data.
/// </summary>
public class HintData : PoolObject
{
    /// <summary>
    /// Gets or sets the vertical offset of the message.
    /// </summary>
    public float VerticalOffset { get; set; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the size of the content.
    /// </summary>
    public int Size { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of this parsing part.
    /// </summary>
    public int Id { get; set; }

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();

        Content = null;
        
        VerticalOffset = 0f;
        Size = 0;
        Id = 0;
    }
}