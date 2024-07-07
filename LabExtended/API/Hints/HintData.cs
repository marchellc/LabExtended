namespace LabExtended.API.Hints
{
    public class HintData
    {
        public readonly float VerticalOffset;
        public readonly string Content;
        public readonly int Size;
        public readonly int Id;

        public HintData(string content, int size, float vOffset, int id)
        {
            VerticalOffset = vOffset;
            Content = content;
            Size = size;
            Id = id;
        }
    }
}