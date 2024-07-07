using LabExtended.API;
using UnityEngine;

namespace LabExtended.API.Interfaces
{
    /// <summary>
    /// Represents an object that has it's position synchronized with all players.
    /// </summary>
    public interface INetworkedPosition
    {
        /// <summary>
        /// Gets the object's position.
        /// </summary>
        Vector3 Position { set; }

        /// <summary>
        /// Gets the object's position for a specific player.
        /// </summary>
        /// <param name="player">The player to get the position for.</param>
        /// <returns>A position specific to this player.</returns>
        Vector3 PositionFor(ExPlayer player);

        /// <summary>
        /// Removes a fake position for a list of players.
        /// </summary>
        /// <param name="args">The list of players to have their fake positions removed.</param>
        void RemovePositionFor(params ExPlayer[] args);

        /// <summary>
        /// Sets a fake position for a list of players.
        /// </summary>
        /// <param name="position">The position to fake to these players.</param>
        /// <param name="args">The list of players to have this position faked.</param>
        void SetPositionFor(Vector3 position, params ExPlayer[] args);
    }
}