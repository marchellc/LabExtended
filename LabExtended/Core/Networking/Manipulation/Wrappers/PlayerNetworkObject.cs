using LabExtended.API;

using Mirror;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

namespace LabExtended.Core.Networking.Manipulation.Wrappers;

/// <summary>
/// Manipulation of players.
/// </summary>
public class PlayerNetworkObject : INetworkObject
{
    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer? Target { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerNetworkObject"/> instance.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PlayerNetworkObject(ExPlayer target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        Target = target;
    }
    
    /// <inheritdoc cref="INetworkObject.Identity"/>
    public NetworkIdentity? Identity => Target?.Identity;

    /// <inheritdoc cref="INetworkObject.Position"/>
    public Vector3 Position
    {
        get => Target?.Position ?? Vector3.zero;
        set => Target?.Position?.Set(value);
    }

    /// <inheritdoc cref="INetworkObject.LocalPosition"/>
    public Vector3 LocalPosition { get; set; }

    /// <inheritdoc cref="INetworkObject.Scale"/>
    public Vector3 Scale
    {
        get => Target?.Scale ?? Vector3.one;
        set => Target!.Scale = value;
    }

    /// <inheritdoc cref="INetworkObject.Rotation"/>
    public Quaternion Rotation
    {
        get => Target?.Rotation ?? Quaternion.identity;
        set => Target?.Rotation?.Set(value);
    }

    /// <inheritdoc cref="INetworkObject.LocalRotation"/>
    public Quaternion LocalRotation { get; set; }

    /// <inheritdoc cref="INetworkObject.IsAlive"/>
    public bool IsAlive => Target != null && Target.ReferenceHub != null;

    /// <inheritdoc cref="INetworkObject.IsFollowable"/>
    public bool IsFollowable => IsAlive && Target.Role.IsAlive;

    /// <inheritdoc cref="INetworkObject.SupportsParenting"/>
    public bool SupportsParenting => false;

    /// <inheritdoc cref="INetworkObject.ChangeProperties"/>
    public void ChangeProperties(Vector3? newPosition, Vector3? newScale, Quaternion? newRotation)
    {
        if (newPosition.HasValue)
            Position = newPosition.Value;
        
        if (newRotation.HasValue)
            Rotation = newRotation.Value;
        
        if (newScale.HasValue)
            Scale = newScale.Value;
    }

    /// <inheritdoc cref="INetworkObject.LookAtDirection"/>
    public void LookAtDirection(Vector3 direction)
    {
        if (Target is null)
            return;

        if (Target.IsDummy)
            Target.Role.MouseLook.LookAtDirection(direction);
        else
            Rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
    
    /// <inheritdoc cref="INetworkObject.ChangeParent"/>
    public void ChangeParent(Transform? newParent) { }

    /// <inheritdoc cref="INetworkObject.ResetObject"/>
    public void ResetObject() { }
}