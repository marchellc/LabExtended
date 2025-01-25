using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class DynamicElement : HintElement
    {
        public DynamicElement() { }
        public DynamicElement(Func<DynamicElement, ExPlayer, bool> content, Action<DynamicElement> update) => (Content, Update) = (content, update);

        public Func<DynamicElement, ExPlayer, bool> Content { get; set; }
        public Action<DynamicElement> Update { get; set; }

        public override void OnUpdate()
            => Update.InvokeSafe(this);

        public override bool OnDraw(ExPlayer player)
            => Content(this, player);
    }
}