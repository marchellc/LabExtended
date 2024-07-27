using UnityEngine;

namespace LabExtended.API.Interfaces
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
    }
}
