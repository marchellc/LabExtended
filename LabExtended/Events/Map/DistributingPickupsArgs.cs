using LabExtended.Core.Events;

using MapGeneration.Distributors;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a spawnable item is distributed.
    /// </summary>
    public class DistributingPickupsArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// The spawnable item.
        /// </summary>
        public SpawnableItem Item { get; }

        /// <summary>
        /// The amount to spawn.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// The list of valid spawn points.
        /// </summary>
        public List<ItemSpawnpoint> SpawnPoints { get; }

        internal DistributingPickupsArgs(SpawnableItem item, int amount, List<ItemSpawnpoint> spawnpoints)
        {
            Item = item;
            Amount = amount;
            SpawnPoints = spawnpoints;
        }
    }
}