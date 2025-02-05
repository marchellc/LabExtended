using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements.Personal;

public class PersonalStaticHintElement : PersonalHintElement
{
    public string Content { get; set; }

    public Action<PersonalStaticHintElement> Update { get; set; }

    public PersonalStaticHintElement() { }
    public PersonalStaticHintElement(Action<PersonalStaticHintElement> update) 
        => Update = update;

    public override void OnUpdate()
        => Update.InvokeSafe(this);

    public override bool OnDraw()
    {
        if (Content is null)
            return false;

        Builder.AppendLine(Content);
        return true;
    }
}