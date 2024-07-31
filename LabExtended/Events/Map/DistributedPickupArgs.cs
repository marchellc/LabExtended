using InventorySystem.Items.Pickups;

using LabExtended.Core.Hooking.Interfaces;

using MapGeneration.Distributors;

namespace LabExtended.Events.Map
{
    public class DistributedPickupArgs : IHookEvent
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