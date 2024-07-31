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
    }
}