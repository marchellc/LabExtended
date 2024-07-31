using LabExtended.Extensions;

namespace LabExtended.API.Hints.Elements
{
    public class DynamicElement : HintElement
    {
        public DynamicElement() { }
        public DynamicElement(Func<ExPlayer, string> content, Action<DynamicElement> update) => (Content, Update) = (content, update);

        public Func<ExPlayer, string> Content { get; set; }
        public Action<DynamicElement> Update { get; set; }

        public override void UpdateElement()
        {
            base.UpdateElement();
            Update.InvokeSafe(this);
        }

        public override string GetContent(ExPlayer player)
            => Content(player);
    }
}