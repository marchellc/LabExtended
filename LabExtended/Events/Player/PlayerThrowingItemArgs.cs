using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries to throw an item.
    /// </summary>
    public class PlayerThrowingItemArgs : BoolCancellableEvent
    {
        /// <summary>
        /// Gets the throwing player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the item to be thrown.
        /// <para>This <b>will</b> be a prefab in case it's called by <see cref="ExPlayer.ThrowItem{T}(ItemType, ushort?)"/>!</para>
        /// </summary>
        public ItemBase Item { get; }

        /// <summary>
        /// Gets the pickup to be thrown.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Gets the pickup's <see cref="UnityEngine.Rigidbody"/>.
        /// </summary>
        public Rigidbody Rigidbody { get; }

        /// <summary>
        /// Gets or sets the starting position.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the item's velocity.
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the item's angular velocity.
        /// </summary>
        public Vector3 AngularVelocity { get; set; }

        internal PlayerThrowingItemArgs(ExPlayer player, ItemBase item, ItemPickupBase pickup, Rigidbody rigidbody, Vector3 position, Vector3 velocity, Vector3 angularVelocity)
        {
            Player = player;
            Item = item;
            Pickup = pickup;
            Rigidbody = rigidbody;
            Position = position;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }
}