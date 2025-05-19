using LabExtended.API.Enums;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Hints.Elements.Personal;

/// <summary>
/// A subtype hint element which assigns elements to specific players rather than globally.
/// </summary>
public class PersonalHintElement : HintElement
{
    /// <summary>
    /// Gets the player that owns this element.
    /// </summary>
    public ExPlayer Player { get; internal set; }

    /// <summary>
    /// Gets the alignment of the element's content.
    /// </summary>
    public virtual HintAlign Alignment { get; } = HintElement.DefaultHintAlign;

    /// <summary>
    /// Gets the vertical offset of the element's content.
    /// </summary>
    public virtual float VerticalOffset { get; } = HintElement.DefaultVerticalOffset;

    /// <summary>
    /// Gets the pixel line spacing of the element's content.
    /// </summary>
    public virtual int PixelSpacing { get; } = HintElement.DefaultPixelLineSpacing;
    
    /// <summary>
    /// Gets called once the element is drawn.
    /// </summary>
    /// <returns>true if the content should be shown</returns>
    public virtual bool OnDraw() 
        => false;

    /// <inheritdoc cref="HintElement.GetAlignment"/>
    public override HintAlign GetAlignment(ExPlayer player) 
        => Alignment;
    
    /// <inheritdoc cref="HintElement.GetPixelSpacing"/>
    public override int GetPixelSpacing(ExPlayer player) 
        => PixelSpacing;
    
    /// <inheritdoc cref="HintElement.GetVerticalOffset"/>
    public override float GetVerticalOffset(ExPlayer player) 
        => VerticalOffset;

    /// <inheritdoc cref="HintElement.OnDraw"/>
    public override bool OnDraw(ExPlayer player)
    {
        if (!Player)
            return false;

        return OnDraw();
    }
}