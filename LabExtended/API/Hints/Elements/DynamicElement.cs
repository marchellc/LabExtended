namespace LabExtended.API.Hints.Elements
{
    public class DynamicElement : HintElement
    {
        public DynamicElement() { }
        public DynamicElement(Func<string> content) { Content = content; }

        public Func<string> Content { get; set; }

        public override string GetContent()
            => Content();
    }
}