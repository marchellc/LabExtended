using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class StaticElement : HintElement
    {
        public string Content { get; set; }

        public Action<StaticElement> Update { get; set; }

        public StaticElement() { }
        public StaticElement(Action<StaticElement> update) => Update = update;

        public override void UpdateElement()
        {
            base.UpdateElement();
            Update.InvokeSafe(this);
        }

        public override string GetContent(ExPlayer _)
            => Content;
    }
}