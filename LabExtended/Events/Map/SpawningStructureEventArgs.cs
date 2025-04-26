using LabApi.Features.Wrappers;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets called when the game starts spawning a structure.
    /// </summary>
    public class SpawningStructureEventArgs : BooleanEventArgs
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
        public Door TriggerDoor { get; set; }

        /// <summary>
        /// Creates a new <see cref="SpawningStructureEventArgs"/> instance.
        /// </summary>
        /// <param name="structure">The structure being spawned.</param>
        /// <param name="transform">The structure's parent transform.</param>
        /// <param name="triggerDoor">The structure's trigger door.</param>
        public SpawningStructureEventArgs(SpawnableStructure structure, Transform transform, Door triggerDoor)
        {
            Structure = structure;
            Transform = transform;
            TriggerDoor = triggerDoor;
        }
    }
}
