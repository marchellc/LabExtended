using InventorySystem.Items.Pickups;

using MapGeneration.Distributors;

namespace LabExtended.Events.Map
{
    public class DistributedPickupArgs
    {
        public ItemSpawnpoint SpawnPoint { get; }

        public ItemPickupBase Pickup { get; }

        internal DistributedPickupArgs(ItemSpawnpoint spawnpoint, ItemPickupBase pickup)
        {
            SpawnPoint = spawnpoint;
            Pickup = pickup;
        }
    }
}