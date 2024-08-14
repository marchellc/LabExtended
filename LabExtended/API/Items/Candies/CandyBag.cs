using InventorySystem.Items.Usables.Scp330;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Wrappers;

namespace LabExtended.API.Items.Candies
{
    public class CandyBag : Wrapper<Scp330Bag>
    {
        internal readonly LockedHashSet<CandyItem> _candies = new LockedHashSet<CandyItem>();
        internal CandyItem _selected;

        public CandyBag(Scp330Bag baseValue, ExPlayer owner) : base(baseValue)
        {
            Owner = owner;
        }

        public ExPlayer Owner { get; }

        public int SelectedIndex
        {
            get => Base.SelectedCandyId;
            set
            {
                if (Base.SelectedCandyId == value || value < 0 || value >= Base.Candies.Count)
                    return;

                Base.SelectCandy(value);
            }
        }

        public CandyKindID SelectedType
        {
            get => (Base.SelectedCandyId == 0 || Base.SelectedCandyId >= Base.Candies.Count) ? CandyKindID.None : Base.Candies[Base.SelectedCandyId];
            set
            {
                if (value is CandyKindID.None)
                {
                    if (Base.SelectedCandyId != 0)
                        Base.SelectCandy(0);

                    return;
                }

                var index = Base.Candies.FindIndex(x => x == value);

                if (index < 0 || index >= Base.Candies.Count || index == Base.SelectedCandyId)
                    return;

                Base.SelectCandy(index);
            }
        }

        public CandyItem SelectedCandy
        {
            get => _selected;
            set
            {
                if (_selected is null)
                {
                    Base.SelectCandy(0);

                    _selected = null;
                }
                else
                {
                    if (value == _selected)
                        return;

                    if (value.Index < Base.Candies.Count || value.Index >= Base.Candies.Count)
                        return;

                    Base.SelectCandy(value.Index);
                }
            }
        }

        public IReadOnlyList<CandyItem> Candies => _candies;
        public IReadOnlyList<CandyKindID> Types => Base.Candies;

        public CandyKindID GetCandyType(int index)
            => Base.Candies.TryGet(index, out var type) ? type : CandyKindID.None;

        public void SetCandyType(int index, CandyKindID type)
        {
            if (index < 0 || index >= Base.Candies.Count || Base.Candies[index] == type)
                return;

            Base.Candies[index] = type;

            Refresh(false);
        }

        public void Refresh(bool removeIfEmpty = true)
        {
            if (Base.Candies.Count > 0)
            {
                Owner.Connection.Send(new SyncScp330Message()
                {
                    Candies = Base.Candies,
                    Serial = Base.ItemSerial
                });

                return;
            }

            if (removeIfEmpty)
                Base.ServerRemoveSelf();
        }

        public void Select(int index)
            => Base.SelectCandy(index);

        public void Select(CandyItem item)
            => Base.SelectCandy(item.Index);

        public void Unselect()
            => Base.SelectCandy(0);

        public void Drop(int index)
            => Base.DropCandy(index);

        public void Drop(CandyItem item)
            => Base.DropCandy(item.Index);
    }
}
