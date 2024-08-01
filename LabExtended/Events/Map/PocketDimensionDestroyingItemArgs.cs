using InventorySystem.Items.Pickups;

using LabExtended.Core.Events;

namespace LabExtended.Events.Map
{
    public class PocketDimensionDestroyingItemArgs : HookBooleanCancellableEventBase
    {
        public ItemPickupBase Item { get; }

        internal PocketDimensionDestroyingItemArgs(ItemPickupBase item)
            => Item = item;
    }
}