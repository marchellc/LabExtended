using InventorySystem.Items.Pickups;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a pickup is distributed.
    /// </summary>
    public class DistributedPickupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the spawned pickup.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Creates a new DistributedPickupEventArgs instance.
        /// </summary>
        /// <param name="pickup">The spawned pickup.</param>
        public DistributedPickupEventArgs(ItemPickupBase pickup)
        {
            Pickup = pickup;
        }
    }
}