using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when the player's position is about to get overriden.
    /// </summary>
    public class PlayerTeleportingArgs : HookBooleanCancellableEventBase
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
        /// Gets or sets the player's delta rotation.
        /// </summary>
        public Vector3 DeltaRotation { get; set; }

        internal PlayerTeleportingArgs(ExPlayer player, Vector3 currentPosition, Vector3 newPosition, Vector3 deltaRotation)
        {
            Player = player;
            CurrentPosition = currentPosition;
            NewPosition = newPosition;
            DeltaRotation = deltaRotation;
        }
    }
}