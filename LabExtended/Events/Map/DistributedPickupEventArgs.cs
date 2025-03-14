using InventorySystem.Items.Pickups;

using MapGeneration.Distributors;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a pickup is distributed.
    /// </summary>
    public class DistributedPickupEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the pickup's spawnpoint.
        /// </summary>
        public ItemSpawnpoint SpawnPoint { get; }
        
        /// <summary>
        /// Gets the spawned pickup.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Creates a new DistributedPickupEventArgs instance.
        /// </summary>
        /// <param name="spawnpoint">The pickup's spawnpoint.</param>
        /// <param name="pickup">The spawned pickup.</param>
        public DistributedPickupEventArgs(ItemSpawnpoint spawnpoint, ItemPickupBase pickup)
        {
            SpawnPoint = spawnpoint;
            Pickup = pickup;
        }
    }
}