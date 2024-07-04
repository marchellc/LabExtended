using LabExtended.API;
using UnityEngine;

namespace LabExtended.Interfaces
{
    /// <summary>
    /// Represents an object that has it's rotation synchronized with all players.
    /// </summary>
    public interface INetworkedRotation
    {
        /// <summary>
        /// Sets the object's rotation.
        /// </summary>
        Quaternion Rotation { set; }

        /// <summary>
        /// Gets the object's rotation for a specific player.
        /// </summary>
        /// <param name="player">The player to get the rotation of this object for.</param>
        /// <returns>The rotation specific to this player.</returns>
        Quaternion RotationFor(ExPlayer player);

        /// <summary>
        /// Sets the object's rotation for specific players.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="args">The list of players to send a different rotation to.</param>
        void SetRotationFor(Quaternion rotation, params ExPlayer[] args);

        /// <summary>
        /// Removes a fake rotation for specific players.
        /// </summary>
        /// <param name="args">The list of players to have their fake rotations removed.</param>
        void RemoveRotationFor(params ExPlayer[] args);
    }
}
