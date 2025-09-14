using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys;

/// <summary>
/// Wrapper for the <see cref="AdminToys.WaypointToy"/> admin toy.
/// </summary>
public class WaypointToy : AdminToy, IWrapper<AdminToys.WaypointToy>
{
    /// <summary>
    /// Spawns a new Waypoint toy.
    /// <param name="position">The spawn position of the waypoint.</param>
    /// <param name="rotation">The spawn rotation of the waypoint.</param>
    /// <param name="priority">The priority of the waypoint.</param>
    /// </summary>
    public WaypointToy(Vector3? position = null, Quaternion? rotation = null, float? priority = null) 
        : base(PrefabList.Waypoint.CreateInstance().GetComponent<AdminToys.WaypointToy>())
    {
        Base = base.Base as AdminToys.WaypointToy!;

        if (Base is null)
            throw new Exception("Could not create a WaypointToy instance!");
        
        if (position.HasValue)
            Base.NetworkPosition = position.Value;

        if (rotation.HasValue)
            Base.NetworkRotation = rotation.Value;
        
        if (priority.HasValue)
            Base.NetworkPriority = priority.Value;
        
        Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation); 
        Base.transform.localScale = Base.NetworkScale = Vector3.one;
        
        NetworkServer.Spawn(Base.gameObject);
    }
    
    internal WaypointToy(AdminToys.WaypointToy baseValue) : base(baseValue)
    {
        Base = baseValue;
    }

    /// <inheritdoc cref="Base"/>
    public new AdminToys.WaypointToy Base { get; }

    /// <summary>
    /// Gets or sets the priority of the waypoint.
    /// </summary>
    public float Priority
    {
        get => Base.Priority;
        set => Base.NetworkPriority = value;
    }

    /// <summary>
    /// Whether or not the show the waypoint's bounds.
    /// </summary>
    public bool ShowBounds
    {
        get => Base.VisualizeBounds;
        set => Base.NetworkVisualizeBounds = value;
    }

    /// <summary>
    /// Gets or sets the size of the waypoint's bounds.
    /// </summary>
    public Vector3 BoundsSize
    {
        get => Base.BoundsSize;
        set => Base.NetworkBoundsSize = value;
    }
}