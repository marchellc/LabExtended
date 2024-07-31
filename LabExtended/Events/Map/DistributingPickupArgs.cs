using LabExtended.Core.Events;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when an item is being spawned at the round start.
    /// </summary>
    public class DistributingPickupArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the targeted spawn point.
        /// </summary>
        public ItemSpawnpoint SpawnPoint { get; }

        /// <summary>
        /// Gets the type of the item to spawn.
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// Gets or sets the transform to spawn the item at.
        /// </summary>
        public Transform TargetTransform { get; set; }

        /// <summary>
        /// Gets or sets a custom spawn position.
        /// </summary>
        public Vector3? TargetPosition { get; set; }

        internal DistributingPickupArgs(ItemSpawnpoint spawnpoint, ItemType type, Transform transform)
        {
            SpawnPoint = spawnpoint;
            Type = type;

            TargetTransform = transform;
            TargetPosition = null;
        }
    }
}