using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class StaticHintElement : HintElement
    {
        public string Content { get; set; }

        public Action<StaticHintElement> Update { get; set; }

        public StaticHintElement() { }
        public StaticHintElement(Action<StaticHintElement> update) 
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