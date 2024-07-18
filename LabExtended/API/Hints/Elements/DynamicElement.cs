namespace LabExtended.API.Hints.Elements
{
    public class DynamicElement : HintElement
    {
        public DynamicElement() { }
        public DynamicElement(Func<ExPlayer, string> content) { Content = content; }

        public Func<ExPlayer, string> Content { get; set; }

        public override string GetContent(ExPlayer player)
            => Content(player);
    }
}