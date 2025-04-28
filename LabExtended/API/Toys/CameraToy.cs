using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using MapGeneration;

using Mirror;

using UnityEngine;

#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.API.Toys;

/// <summary>
/// Represents a spawnable SCP-079 camera toy.
/// </summary>
public class CameraToy : AdminToy, IWrapper<Scp079CameraToy>
{
    /// <summary>
    /// Spawns a new SCP-079 camera toy.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public CameraToy() : base(PrefabList.EzCamera.CreateInstance().GetComponent<AdminToyBase>())
    {
        Base = base.Base as Scp079CameraToy;
        
        if (Base is null)
            throw new Exception("Could not spawn Scp079CameraToy");
        
        Camera = Camera.Get(Base._camera);
        
        NetworkServer.Spawn(Base.gameObject);
    }

    internal CameraToy(Scp079CameraToy baseValue) : base(baseValue)
    {
        Base = baseValue;
        Camera = Camera.Get(baseValue._camera);
    }

    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new Scp079CameraToy Base { get; }
    
    /// <summary>
    /// Gets the paired camera component.
    /// </summary>
    public Camera Camera { get; }

    /// <summary>
    /// Gets or sets the camera's label.
    /// </summary>
    public string Label
    {
        get => Base.Label;
        set => Base.NetworkLabel = value;
    }

    /// <summary>
    /// Gets or sets the camera's room.
    /// </summary>
    public RoomIdentifier Room
    {
        get => Base.Room;
        set => Base.NetworkRoom = value;
    }

    /// <summary>
    /// Gets or sets the camera's horizontal constraint.
    /// </summary>
    public Vector2 HorizontalConstraint
    {
        get => Base.HorizontalConstraint;
        set => Base.NetworkHorizontalConstraint = value;
    }

    /// <summary>
    /// Gets or sets the camera's vertical constraint.
    /// </summary>
    public Vector2 VerticalConstraint
    {
        get => Base.VerticalConstraint;
        set => Base.NetworkVerticalConstraint = value;
    }

    /// <summary>
    /// Gets or sets the camera's zoom constraint.
    /// </summary>
    public Vector2 ZoomConstraint
    {
        get => Base.ZoomConstraint;
        set => Base.NetworkZoomConstraint = value;
    }
}