using MapGeneration.Distributors;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when a spawnable item is distributed.
    /// </summary>
    public class DistributingPickupsEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The amount to spawn.
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// The spawnable item.
        /// </summary>
        public SpawnableItem Item { get; }

        /// <summary>
        /// The list of valid spawn points.
        /// </summary>
        public List<ItemSpawnpoint> SpawnPoints { get; }

        /// <summary>
        /// Creates a new <see cref="DistributingPickupsEventArgs"/> instance.
        /// </summary>
        /// <param name="item">The item spawn configuration.</param>
        /// <param name="amount">The amount that should be spawn.</param>
        /// <param name="spawnpoints">The targeted item spawnpoints.</param>
        public DistributingPickupsEventArgs(SpawnableItem item, int amount, List<ItemSpawnpoint> spawnpoints) : base(true)
        {
            Item = item;
            Amount = amount;
            SpawnPoints = spawnpoints;
        }
    }
}