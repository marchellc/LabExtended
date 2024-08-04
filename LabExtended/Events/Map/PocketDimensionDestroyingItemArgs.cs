using InventorySystem.Items.Pickups;

using LabExtended.Core.Events;

namespace LabExtended.Events.Map
{
    public class PocketDimensionDestroyingItemArgs : BoolCancellableEvent
    {
        public ItemPickupBase Item { get; }

        internal PocketDimensionDestroyingItemArgs(ItemPickupBase item)
            => Item = item;
    }
}