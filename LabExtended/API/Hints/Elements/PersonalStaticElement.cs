using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements;

public class PersonalStaticElement : PersonalElement
{
    public string Content { get; set; }

    public Action<PersonalStaticElement> Update { get; set; }

    public PersonalStaticElement() { }
    public PersonalStaticElement(Action<PersonalStaticElement> update) 
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