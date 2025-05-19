using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements.Personal;

/// <summary>
/// A subtype personal hint element which allows you to define per-player content.
/// </summary>
public class PersonalDynamicHintElement : PersonalHintElement
{
    /// <summary>
    /// Creates a new <see cref="PersonalDynamicHintElement"/> instance.
    /// </summary>
    public PersonalDynamicHintElement() { }
    
    /// <summary>
    /// Creates a new <see cref="PersonalDynamicHintElement"/> instance.
    /// </summary>
    /// <param name="content">The method used to retrieve the content of for this player.</param>
    /// <param name="update">The method used to update the element.</param>
    public PersonalDynamicHintElement(Func<PersonalDynamicHintElement, bool> content, Action<PersonalDynamicHintElement> update) 
        => (Content, Update) = (content, update);

    /// <summary>
    /// Gets or sets the method used to retrieve the content. Should return true if the content should be displayed.
    /// </summary>
    public Func<PersonalDynamicHintElement, bool> Content { get; set; }
    
    /// <summary>
    /// Gets or sets the method used to update the element.
    /// </summary>
    public Action<PersonalDynamicHintElement> Update { get; set; }

    /// <inheritdoc cref="HintElement.OnUpdate"/>
    public override void OnUpdate()
        => Update.InvokeSafe(this);

    /// <inheritdoc cref="PersonalHintElement.OnDraw()"/>
    public override bool OnDraw()
        => Content(this);
}