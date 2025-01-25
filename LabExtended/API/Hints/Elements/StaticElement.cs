using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class StaticElement : HintElement
    {
        public string Content { get; set; }

        public Action<StaticElement> Update { get; set; }

        public StaticElement() { }
        public StaticElement(Action<StaticElement> update) 
            => Update = update;

        public override void OnUpdate()
            => Update.InvokeSafe(this);

        public override bool OnDraw(ExPlayer _)
        {
            if (Content is null)
                return false;
            
            Builder.AppendLine(Content);
            return true;
        }
    }
}