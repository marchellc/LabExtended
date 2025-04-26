using InventorySystem.Items.Pickups;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when an item is being spawned at the round start.
    /// </summary>
    public class DistributingPickupEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the pickup to be spawned.
        /// </summary>
        public ItemPickupBase Pickup { get; set; }

        /// <summary>
        /// Creates a new DistributingPickupEventArgs instance.
        /// </summary>
        /// <param name="pickup">The pickup to spawn.</param>
        public DistributingPickupEventArgs(ItemPickupBase pickup) : base(true)
        {
            Pickup = pickup;
        }
    }
}