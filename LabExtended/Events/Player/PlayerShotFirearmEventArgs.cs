﻿using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules.Misc;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a firearm calculates target hits.
/// </summary>
public class PlayerShotFirearmEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player shooting the firearm.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the target player which is getting hit.
    /// </summary>
    public ExPlayer? TargetPlayer { get; }
    
    /// <summary>
    /// Gets the firearm which is being shot.
    /// </summary>
    public Firearm Firearm { get; }
    
    /// <summary>
    /// Gets the raycast which was used.
    /// </summary>
    public HitscanResult Hitscan { get; }
    
    /// <summary>
    /// Gets the ray which was used to cast.
    /// </summary>
    public Ray OriginRay { get; }
    
    /// <summary>
    /// Gets the raycast hit.
    /// </summary>
    public RaycastHit Hit { get; }
    
    /// <summary>
    /// Gets the target which is getting damaged (can be null).
    /// </summary>
    public IDestructible? Target { get; }

    /// <summary>
    /// Gets or sets the damage which is going to be dealt.
    /// </summary>
    public float TargetDamage { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerShootingFirearmEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player shooting the firearm.</param>
    /// <param name="firearm">The firearm which is being shot.</param>
    /// <param name="target">The target which is getting hit.</param>
    /// <param name="ray">The ray used to cast.</param>
    /// <param name="hit">The cast raycast hit.</param>
    /// <param name="damage">The damage to deal to the target.</param>
    /// <param name="hitscan">The raycast which was used.</param>
    /// <param name="targetPlayer">The player who got hit.</param>
    public PlayerShotFirearmEventArgs(ExPlayer player, Firearm firearm, IDestructible? target, Ray ray, RaycastHit hit, float damage, 
        HitscanResult hitscan, ExPlayer targetPlayer)
    {
        Player = player;
        Firearm = firearm;
        Hitscan = hitscan;
        
        OriginRay = ray;
        Hit = hit;
        
        Target = target;
        TargetDamage = damage;
        TargetPlayer = targetPlayer;
    }
}