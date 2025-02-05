using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class DynamicHintElement : HintElement
    {
        public DynamicHintElement() { }
        public DynamicHintElement(Func<DynamicHintElement, ExPlayer, bool> content, Action<DynamicHintElement> update) => (Content, Update) = (content, update);

        public Func<DynamicHintElement, ExPlayer, bool> Content { get; set; }
        public Action<DynamicHintElement> Update { get; set; }

        public override void OnUpdate()
            => Update.InvokeSafe(this);

        public override bool OnDraw(ExPlayer player)
            => Content(this, player);
    }
}