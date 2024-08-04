using InventorySystem.Items.Pickups;

using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Map
{
    public class PocketDimensionDroppingItemArgs : BoolCancellableEvent
    {
        public ItemPickupBase Item { get; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        internal PocketDimensionDroppingItemArgs(ItemPickupBase item, Vector3 pos, Vector3 vel)
            => (Item, Position, Velocity) = (item, pos, vel);
    }
}