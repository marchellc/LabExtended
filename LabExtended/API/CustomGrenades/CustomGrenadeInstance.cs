using LabExtended.API.CustomItems;
using UnityEngine;
using Utils;

namespace LabExtended.API.CustomGrenades;

/// <summary>
/// Represents a custom grenade.
/// </summary>
public class CustomGrenadeInstance : CustomItemInstance
{
    /// <summary>
    /// Gets the custom data.
    /// </summary>
    public new CustomGrenadeData? CustomData => base.CustomData as CustomGrenadeData;

    /// <summary>
    /// Gets or sets the remaining time.
    /// </summary>
    public float RemainingTime { get; set; } = 0f;

    /// <summary>
    /// Gets the time of the grenade throw start.
    /// </summary>
    public float ReadyTime { get; internal set; } = 0f;

    /// <summary>
    /// Gets the time of the grenade spawning.
    /// </summary>
    public float SpawnTime { get; internal set; } = 0f;

    /// <summary>
    /// Gets the time of the grenade detonating.
    /// </summary>
    public float DetonationTime { get; internal set; } = 0f;
    
    /// <summary>
    /// Gets the amount of seconds that the grenade was spawned for.
    /// </summary>
    public float SpawnedFor => Time.timeSinceLevelLoad - SpawnTime;
    
    /// <summary>
    /// Whether or not the grenade is ready to be thrown.
    /// </summary>
    public bool IsReady { get; internal set; }

    /// <summary>
    /// Whether or not the grenade is spawned.
    /// </summary>
    public bool IsSpawned { get; internal set; }
    
    /// <summary>
    /// Whether or not the grenade has detonated.
    /// </summary>
    public bool IsDetonated { get; internal set; }

    /// <summary>
    /// Called once a grenade is about to be thrown.
    /// </summary>
    /// <returns>true if the grenade should be thrown.</returns>
    public virtual bool OnSpawning() => true;
    
    /// <summary>
    /// Called once a grenade is thrown.
    /// </summary>
    public virtual void OnSpawned() { }

    /// <summary>
    /// Called once the detonation starts.
    /// </summary>
    /// <returns></returns>
    public virtual bool OnDetonating() => true;
    
    /// <summary>
    /// Called once the grenade detonates.
    /// </summary>
    public virtual void OnDetonated() { }

    /// <summary>
    /// Instantly detonates this grenade.
    /// </summary>
    public void Activate()
    {
        if (!IsSpawned)
            throw new Exception("This Custom Grenade has not been thrown yet.");
        
        if (IsDetonated)
            throw new Exception("This Custom Grenade has already been detonated.");

        RemainingTime = 0f;
    }

    /// <summary>
    /// Spawns an explosion of the specific type.
    /// </summary>
    /// <param name="explosionType">The type to spawn.</param>
    public void SpawnExplosion(ItemType explosionType)
        => ExplosionUtils.ServerSpawnEffect(Pickup.Position, explosionType);
}