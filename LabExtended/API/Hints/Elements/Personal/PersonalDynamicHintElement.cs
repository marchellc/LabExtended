using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements.Personal;

public class PersonalDynamicHintElement : PersonalHintElement
{
    public PersonalDynamicHintElement() { }
    public PersonalDynamicHintElement(Func<PersonalDynamicHintElement, bool> content, Action<PersonalDynamicHintElement> update) => (Content, Update) = (content, update);

    public Func<PersonalDynamicHintElement, bool> Content { get; set; }
    public Action<PersonalDynamicHintElement> Update { get; set; }

    public override void OnUpdate()
        => Update.InvokeSafe(this);

    public override bool OnDraw()
        => Content(this);
}