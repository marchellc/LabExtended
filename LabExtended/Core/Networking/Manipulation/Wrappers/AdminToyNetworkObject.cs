using LabExtended.API.Toys;

using Mirror;

using UnityEngine;

namespace LabExtended.Core.Networking.Manipulation.Wrappers;

/// <summary>
/// Manipulation of admin toys.
/// </summary>
public class AdminToyNetworkObject : INetworkObject
{
    /// <summary>
    /// Gets the target toy.
    /// </summary>
    public AdminToy? Target { get; }

    /// <summary>
    /// Creates a new <see cref="AdminToyNetworkObject"/> instance.
    /// </summary>
    /// <param name="target">The target admin toy.</param>
    public AdminToyNetworkObject(AdminToy target)
    {
        if (target is null || target.Base == null)
            throw new ArgumentNullException(nameof(target));
                
        Target = target;
    }
    
    /// <inheritdoc cref="INetworkObject.Identity"/>
    public NetworkIdentity? Identity => Target?.Identity;

    /// <inheritdoc cref="INetworkObject.Position"/>
    public Vector3 Position
    {
        get => Target?.Position ?? Vector3.zero;
        set => Target!.Position = value;
    }

    /// <inheritdoc cref="INetworkObject.LocalPosition"/>
    public Vector3 LocalPosition
    {
        get => Target?.Transform.localPosition ?? Vector3.zero;
        set => Target!.Transform.localPosition = value;
    }

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
        set => Target!.Rotation = value;
    }

    /// <inheritdoc cref="INetworkObject.LocalRotation"/>
    public Quaternion LocalRotation
    {
        get => Target?.Transform.localRotation ?? Quaternion.identity;
        set => Target!.Transform.localRotation = value;
    }

    /// <inheritdoc cref="INetworkObject.IsAlive"/>
    public bool IsAlive => Target != null && Target.Base != null;

    /// <inheritdoc cref="INetworkObject.IsFollowable"/>
    public bool IsFollowable => IsAlive;

    /// <inheritdoc cref="INetworkObject.SupportsParenting"/>
    public bool SupportsParenting => true;

    /// <inheritdoc cref="INetworkObject.ChangeProperties"/>
    public void ChangeProperties(Vector3? newPosition, Vector3? newScale, Quaternion? newRotation)
    {
        if (newPosition.HasValue)
            Position = newPosition.Value;
        
        if (newScale.HasValue)
            Scale = newScale.Value;
        
        if (newRotation.HasValue)
            Rotation = newRotation.Value;
    }

    /// <inheritdoc cref="INetworkObject.ChangeParent"/>
    public void ChangeParent(Transform? newParent)
    {
        Target?.SetParent(newParent);
    }

    /// <inheritdoc cref="INetworkObject.ResetObject"/>
    public void ResetObject()
    {
        Target?.SetParent(default(Transform));
    }

    /// <inheritdoc cref="INetworkObject.LookAtDirection"/>
    public void LookAtDirection(Vector3 direction)
    {
        Rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}