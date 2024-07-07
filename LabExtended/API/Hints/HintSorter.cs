namespace LabExtended.API.Hints
{
    public class HintSorter : IComparer<HintData>
    {
        public int Compare(HintData x, HintData y)
        {
            if (x.VerticalOffset != y.VerticalOffset)
                return (int)(x.VerticalOffset - y.VerticalOffset);

            return x.Id - y.Id;
        }
    }
}