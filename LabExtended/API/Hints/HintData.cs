using LabExtended.Core.Pooling;

namespace LabExtended.API.Hints
{
    public class HintData : PoolObject
    {
        public float VerticalOffset { get; set; }
        
        public string Content { get; set; }
        
        public int Size { get; set; }
        public int Id { get; set; }

        public override void OnReturned()
        {
            base.OnReturned();

            VerticalOffset = 0f;
            Content = null;
            Size = 0;
            Id = 0;
        }
    }
}