using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements;

public class PersonalDynamicElement : PersonalElement
{
    public PersonalDynamicElement() { }
    public PersonalDynamicElement(Func<PersonalDynamicElement, bool> content, Action<PersonalDynamicElement> update) => (Content, Update) = (content, update);

    public Func<PersonalDynamicElement, bool> Content { get; set; }
    public Action<PersonalDynamicElement> Update { get; set; }

    public override void OnUpdate()
        => Update.InvokeSafe(this);

    public override bool OnDraw()
        => Content(this);
}