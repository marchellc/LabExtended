using AdminToys;

using LabExtended.API.Interfaces;
using LabExtended.API.Prefabs;
using Mirror;
using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.API.Toys;

/// <summary>
/// A wrapper for <see cref="AdminToys.CapybaraToy"/>.
/// </summary>
public class CapybaraToy : AdminToy, IWrapper<AdminToys.CapybaraToy>
{
    /// <summary>
    /// Spawns a new capybara toy.
    /// <param name="position">The position to spawn the toy at.</param>
    /// <param name="rotation">The rotation to spawn the toy with.</param>
    /// </summary>
    public CapybaraToy(Vector3? position = null, Quaternion? rotation = null)
        : base(PrefabList.Capybara.CreateInstance().GetComponent<AdminToyBase>())
    {
        Base = base.Base as AdminToys.CapybaraToy;

        if (Base is null)
            throw new Exception("Could not spawn CapybaraToy");
        
        Base.SpawnerFootprint = ExPlayer.Host.Footprint;
        
        Base.NetworkPosition = position ?? Vector3.zero;
        Base.NetworkRotation = rotation ?? Quaternion.identity;
        
        Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation);
        
        NetworkServer.Spawn(Base.gameObject);
    }

    internal CapybaraToy(AdminToys.CapybaraToy baseValue) : base(baseValue)
        => Base = baseValue;

    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new AdminToys.CapybaraToy Base { get; }

    /// <summary>
    /// Whether collisions are enabled.
    /// </summary>
    public bool CollisionsEnabled
    {
        get => Base._collisionsEnabled;
        set => Base.CollisionsEnabled = value;
    }
}