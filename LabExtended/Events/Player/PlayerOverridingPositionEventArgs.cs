using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when the player's position is about to get overriden.
    /// </summary>
    public class PlayerOverridingPositionEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the teleporting player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the player's current position.
        /// </summary>
        public Vector3 CurrentPosition { get; }

        /// <summary>
        /// Gets or sets the new position.
        /// </summary>
        public Vector3 NewPosition { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerOverridingPositionEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The teleporting player.</param>
        /// <param name="currentPosition">Current position of the player.</param>
        /// <param name="newPosition">New position of the player.</param>
        public PlayerOverridingPositionEventArgs(ExPlayer player, Vector3 currentPosition, Vector3 newPosition)
        {
            Player = player;
            NewPosition = newPosition;
            CurrentPosition = currentPosition;
        }
    }
}