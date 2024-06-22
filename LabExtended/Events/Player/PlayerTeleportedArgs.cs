using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

using UnityEngine;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when the player's position gets overriden.
    /// </summary>
    public class PlayerTeleportedArgs : IHookEvent
    {
        /// <summary>
        /// Gets the teleporting player.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the player's current position.
        /// </summary>
        public Vector3 PreviousPosition { get; }

        /// <summary>
        /// Gets or sets the new position.
        /// </summary>
        public Vector3 NewPosition { get; }

        /// <summary>
        /// Gets or sets the player's delta rotation.
        /// </summary>
        public Vector3 DeltaRotation { get; }

        internal PlayerTeleportedArgs(ExPlayer player, Vector3 prevPosition, Vector3 newPosition, Vector3 deltaRotation)
        {
            Player = player;
            PreviousPosition = prevPosition;
            NewPosition = newPosition;
            DeltaRotation = deltaRotation;
        }
    }
}