using LabExtended.Core.Events;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Events.Map
{
    /// <summary>
    /// Gets triggered when a locker is filling it's chamber.
    /// </summary>
    public class LockerSpawningPickupArgs : BoolCancellableEvent
    {
        /// <summary>
        /// The locker that's filling chambers.
        /// </summary>
        public Locker Locker { get; }

        /// <summary>
        /// The chamber that is being filled.
        /// </summary>
        public LockerChamber Chamber { get; }

        /// <summary>
        /// The item's spawn point.
        /// </summary>
        public Transform SpawnPoint { get; }

        /// <summary>
        /// The amount of the item to spawn.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// The type of the item to spawn.
        /// </summary>
        public ItemType Type { get; set; }

        internal LockerSpawningPickupArgs(Locker locker, LockerChamber chamber, Transform spawnPoint, int amount, ItemType type)
        {
            Locker = locker;
            Chamber = chamber;
            SpawnPoint = spawnPoint;
            Amount = amount;
            Type = type;
        }
    }
}