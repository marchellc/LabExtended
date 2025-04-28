using InventorySystem.Items.Pickups;
using LabExtended.Extensions;
using Mirror;

using UnityEngine;

namespace LabExtended.Core.Networking.Manipulation.Wrappers;

/// <summary>
/// Manipulation of pickups.
/// </summary>
public class ItemPickupNetworkObject : INetworkObject
{
    /// <summary>
    /// Gets the target pickup.
    /// </summary>
    public ItemPickupBase Target { get; }

    /// <summary>
    /// Creates a new <see cref="ItemPickupNetworkObject"/> instance.
    /// </summary>
    /// <param name="target">The target pickup.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ItemPickupNetworkObject(ItemPickupBase target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        
        Target = target;
        Target.FreezePickup();
    }

    /// <inheritdoc cref="INetworkObject.Identity"/>
    public NetworkIdentity? Identity => Target?.netIdentity;

    /// <inheritdoc cref="INetworkObject.Position"/>
    public Vector3 Position
    {
        get => Target?.Position ?? Vector3.zero;
        set => Target!.Position = value;
    }
    
    /// <inheritdoc cref="INetworkObject.Position"/>
    public Vector3 LocalPosition
    {
        get => Target?.transform.localPosition ?? Vector3.zero;
        set => Target!.transform.localPosition = value;
    }

    /// <inheritdoc cref="INetworkObject.Scale"/>
    public Vector3 Scale
    {
        get => Target?.transform.localScale ?? Vector3.zero;
        set
        {
            if (Target is null)
                return;
            
            NetworkServer.UnSpawn(Target.gameObject);
            
            Target.transform.localScale = value;
            
            NetworkServer.Spawn(Target.gameObject);
        }
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
        get => Target?.transform.localRotation ?? Quaternion.identity;
        set => Target!.transform.localRotation = value;
    }

    /// <inheritdoc cref="INetworkObject.SupportsParenting"/>
    public bool SupportsParenting => true;

    /// <inheritdoc cref="INetworkObject.IsAlive"/>
    public bool IsAlive => Target != null;

    /// <inheritdoc cref="INetworkObject.IsFollowable"/>
    public bool IsFollowable => Target != null;

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

    
    /// <inheritdoc cref="INetworkObject.ChangeParent"/>
    public void ChangeParent(Transform? newParent)
    {
        if (Target != null)
            Target.transform.parent = newParent;
    }

    /// <inheritdoc cref="INetworkObject.LookAtDirection"/>
    public void LookAtDirection(Vector3 direction)
    {
        if (Target != null)
            Target.Rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    /// <inheritdoc cref="INetworkObject.ResetObject"/>
    public void ResetObject()
    {
        if (Target != null)
        {
            Target.transform.parent = null;
            
            Target.transform.localPosition = Vector3.zero;
            Target.transform.localRotation = Quaternion.identity;

            Target.UnfreezePickup();
        }
    }
}