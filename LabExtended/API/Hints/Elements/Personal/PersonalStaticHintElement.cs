using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Hints.Elements.Personal;

/// <summary>
/// A subtype personal hint element which provides you with a content property that is set only once.
/// </summary>
public class PersonalStaticHintElement : PersonalHintElement
{
    /// <summary>
    /// Gets or sets the content of the element.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the method used to update the element.
    /// </summary>
    public Action<PersonalStaticHintElement> Update { get; set; }

    /// Creates a new <see cref="PersonalStaticHintElement"/> instance.
    public PersonalStaticHintElement() { }
    
    /// <summary>
    /// Creates a new <see cref="PersonalStaticHintElement"/> instance.
    /// </summary>
    /// <param name="update">The method used to update the element.</param>
    public PersonalStaticHintElement(Action<PersonalStaticHintElement> update) 
        => Update = update;

    /// <inheritdoc cref="HintElement.OnUpdate"/>
    public override void OnUpdate()
        => Update.InvokeSafe(this);

    /// <inheritdoc cref="PersonalHintElement.OnDraw()"/>
    public override bool OnDraw()
    {
        if (Content is null)
            return false;

        if (Builder is null)
            return false;

        Builder.AppendLine(Content);
        return true;
    }
}