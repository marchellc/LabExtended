using LabExtended.Extensions;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Hints.Elements;

/// <summary>
/// A subtype hint element that allows you to provide per-player hint content without a custom element.
/// </summary>
public class DynamicHintElement : HintElement
{
    /// <summary>
    /// Creates a new <see cref="DynamicHintElement"/> instance.
    /// </summary>
    public DynamicHintElement() { }

    /// <summary>
    /// Creates a new <see cref="DynamicHintElement"/> instance.
    /// </summary>
    /// <param name="content">The method used to retrieve per-player content.</param>
    /// <param name="update">The method used to update the element.</param>
    public DynamicHintElement(Func<DynamicHintElement, ExPlayer, bool> content, Action<DynamicHintElement> update) =>
        (Content, Update) = (content, update);

    /// <summary>
    /// Gets or sets the method used to retrieve per-player content.
    /// </summary>
    public Func<DynamicHintElement, ExPlayer, bool> Content { get; set; }
    
    /// <summary>
    /// Gets the method used to update this element.
    /// </summary>
    public Action<DynamicHintElement> Update { get; set; }

    /// <inheritdoc cref="HintElement.OnUpdate"/>
    public override void OnUpdate()
        => Update.InvokeSafe(this);

    /// <inheritdoc cref="HintElement.OnDraw"/>
    public override bool OnDraw(ExPlayer player)
        => Content(this, player);
}