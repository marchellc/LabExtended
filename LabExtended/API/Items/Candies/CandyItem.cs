using InventorySystem.Items.Usables.Scp330;

namespace LabExtended.API.Items.Candies
{
    public class CandyItem
    {
        public CandyItem(CandyBag bag, int index)
        {
            Bag = bag;
            Index = index;
        }

        public CandyBag Bag { get; }

        public int Index { get; }

        public bool IsSelected => Bag.SelectedCandy == this;

        public CandyKindID Type
        {
            get => Bag.GetCandyType(Index);
            set => Bag.SetCandyType(Index, value);
        }

        public void Drop()
            => Bag.Drop(this);

        public void Select()
            => Bag.Select(this);
    }
}