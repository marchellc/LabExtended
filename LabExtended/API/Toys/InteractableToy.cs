using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.API.Toys;

/// <summary>
/// Represents an invisible, interactable admin toy.
/// </summary>
public class InteractableToy : AdminToy, IWrapper<InvisibleInteractableToy>, IDisposable
{
    /// <summary>
    /// Spawns a new interactable toy.
    /// <param name="position">The toy's spawn position.</param>
    /// <param name="rotation">The toy's spawn rotation.</param>
    /// </summary>
    /// <exception cref="Exception"></exception>
    public InteractableToy(Vector3? position = null, Quaternion? rotation = null) 
        : base(PrefabList.Interactable.CreateInstance().GetComponent<AdminToyBase>())
    {
        Base = base.Base as InvisibleInteractableToy;
        
        if (Base is null)
            throw new Exception("Could not spawn InvisibleInteractableToy");

        Shape = InvisibleInteractableToy.ColliderShape.Box;
        InteractionDuration = 0f;
        
        Base.SpawnerFootprint = ExPlayer.Host.Footprint;
        
        Base.NetworkPosition = position ?? Vector3.zero;
        Base.NetworkRotation = rotation ?? Quaternion.identity;
        
        Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation);
        
        NetworkServer.Spawn(Base.gameObject);
    }

    internal InteractableToy(InvisibleInteractableToy baseValue) : base(baseValue)
        => Base = baseValue;
    
    /// <summary>
    /// Gets the toy's collider.
    /// </summary>
    public Collider Collider => Base._collider;
    
    /// <summary>
    /// Gets the toy's custom data storage.
    /// </summary>
    public PlayerStorage? DataStorage { get; private set; } = new(false, null);
    
    /// <summary>
    /// Gets or sets the toy's custom ID.
    /// </summary>
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets or sets the toy's custom data.
    /// </summary>
    public object? CustomData { get; set; } = null;

    /// <summary>
    /// Whether or not the toy can be interacted with.
    /// </summary>
    public bool CanInteract => Base.CanSearch;

    /// <summary>
    /// Whether or not the toy is locked.
    /// </summary>
    public bool IsLocked
    {
        get => Base.IsLocked;
        set => Base.NetworkIsLocked = value;
    }

    /// <summary>
    /// Gets or sets the amount of seconds it takes for an interaction to finish.
    /// </summary>
    public float InteractionDuration
    {
        get => Base.InteractionDuration;
        set => Base.NetworkInteractionDuration = value;
    }

    /// <summary>
    /// Gets or sets the shape of the toy's collider.
    /// </summary>
    public InvisibleInteractableToy.ColliderShape Shape
    {
        get => Base.Shape;
        set => Base.NetworkShape = value;
    }
    
    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new InvisibleInteractableToy Base { get; }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (DataStorage != null)
        {
            DataStorage.Dispose();
            DataStorage = null;
        }
    }
}