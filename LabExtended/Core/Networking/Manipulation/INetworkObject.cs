using Mirror;

using UnityEngine;

namespace LabExtended.Core.Networking.Manipulation;

/// <summary>
/// Represents a network object.
/// </summary>
public interface INetworkObject
{
    /// <summary>
    /// Gets the object's identity.
    /// </summary>
    NetworkIdentity? Identity { get; }
    
    /// <summary>
    /// Gets or sets the object's position.
    /// </summary>
    Vector3 Position { get; set; }
    
    /// <summary>
    /// Gets or sets the object's local position.
    /// </summary>
    Vector3 LocalPosition { get; set; }
    
    /// <summary>
    /// Gets or sets the object's scale.
    /// </summary>
    Vector3 Scale { get; set; }
    
    /// <summary>
    /// Gets or sets the object's rotation.
    /// </summary>
    Quaternion Rotation { get; set; }
    
    /// <summary>
    /// Gets or sets the object's local rotation.
    /// </summary>
    Quaternion LocalRotation { get; set; }
    
    /// <summary>
    /// Whether or not the target object is still alive.
    /// </summary>
    bool IsAlive { get; }
    
    /// <summary>
    /// Whether or not the target object can be followed.
    /// </summary>
    bool IsFollowable { get; }
    
    /// <summary>
    /// Whether or not the target supports synchronized parents.
    /// </summary>
    bool SupportsParenting { get; }
    
    /// <summary>
    /// Changes all supplied properties.
    /// </summary>
    /// <param name="newPosition">Position to set.</param>
    /// <param name="newScale">Scale to set.</param>
    /// <param name="newRotation">Rotation to set.</param>
    void ChangeProperties(Vector3? newPosition, Vector3? newScale, Quaternion? newRotation);
    
    /// <summary>
    /// Changes the object's parent.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    void ChangeParent(Transform? newParent);

    /// <summary>
    /// Resets all changes made to the object.
    /// </summary>
    void ResetObject();
    
    /// <summary>
    /// Sets the object's rotation to look in a specific direction.
    /// </summary>
    /// <param name="direction">The direction to look at.</param>
    void LookAtDirection(Vector3 direction);
}