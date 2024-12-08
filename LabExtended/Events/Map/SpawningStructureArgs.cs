using Interactables.Interobjects.DoorUtils;

using LabExtended.Core.Events;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when the game starts spawning a structure.
    /// </summary>
    public class SpawningStructureArgs : BoolCancellableEvent
    {
        /// <summary>
        /// The structure to spawn.
        /// </summary>
        public SpawnableStructure Structure { get; }

        /// <summary>
        /// The structure's spawnpoint.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// The door that needs to be opened for this structure to spawn.
        /// </summary>
        public DoorVariant TriggerDoor { get; set; }

        internal SpawningStructureArgs(SpawnableStructure structure, Transform transform, DoorVariant triggerDoor)
        {
            Structure = structure;
            Transform = transform;
            TriggerDoor = triggerDoor;
        }
    }
}
