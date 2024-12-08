using UnityEngine;

namespace LabExtended.API.Interfaces
{
    /// <summary>
    /// Represents an object that has a position.
    /// </summary>
    public interface IPosition
    {
        /// <summary>
        /// Gets the object's current position.
        /// </summary>
        Vector3 Position { get; }
    }
}