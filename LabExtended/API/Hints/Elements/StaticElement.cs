namespace LabExtended.API.Hints.Elements
{
    public class StaticElement : HintElement
    {
        public string Content { get; set; }

        public override string GetContent()
            => Content;
    }
}