using LabApi.Features.Wrappers;
using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries to throw an item.
    /// </summary>
    public class PlayerThrowingItemEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the throwing player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the item to be thrown.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// Gets the pickup to be thrown.
        /// </summary>
        public Pickup Pickup { get; }

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

        /// <summary>
        /// Creates a new <see cref="PlayerThrowingItemEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player throwing an item.</param>
        /// <param name="item">The item being thrown.</param>
        /// <param name="pickup">Pickup of the item being thrown.</param>
        /// <param name="rigidbody">Rigidbody component of the item's pickup.</param>
        /// <param name="position">The starting position of the pickup.</param>
        /// <param name="velocity">The pickup's velocity.</param>
        /// <param name="angularVelocity">The pickup's angular velocity.</param>
        public PlayerThrowingItemEventArgs(ExPlayer player, Item item, Pickup pickup, Rigidbody rigidbody, 
            Vector3 position, Vector3 velocity, Vector3 angularVelocity)
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