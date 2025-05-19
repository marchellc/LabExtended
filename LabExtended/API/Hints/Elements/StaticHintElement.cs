using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Hints.Elements;

/// <summary>
/// A subtype hint element that provides you with a string property used as the hint's content.
/// </summary>
public class StaticHintElement : HintElement
{
    /// <summary>
    /// Gets or sets the content of the hint.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the method used to update this element.
    /// </summary>
    public Action<StaticHintElement> Update { get; set; }

    /// <summary>
    /// Creates a new <see cref="StaticHintElement"/> instance.
    /// </summary>
    public StaticHintElement() { }

    /// <summary>
    /// Creates a new <see cref="StaticHintElement"/> instance.
    /// </summary>
    /// <param name="update">The method used to update the element.</param>
    public StaticHintElement(Action<StaticHintElement> update)
        => Update = update;

    /// <inheritdoc cref="HintElement.OnUpdate"/>
    public override void OnUpdate()
        => Update.InvokeSafe(this);

    /// <inheritdoc cref="HintElement.OnDraw"/>
    public override bool OnDraw(ExPlayer _)
    {
        if (Content is null)
            return false;

        Builder.AppendLine(Content);
        return true;
    }
}