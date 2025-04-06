using InventorySystem.Items.Firearms;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a firearm is shot.
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
    public RaycastHit RaycastHit { get; }
    
    /// <summary>
    /// Gets the target which is getting damaged (can be null).
    /// </summary>
    public IDestructible? Target { get; }
    
    /// <summary>
    /// Gets or sets the damage which has been dealt.
    /// </summary>
    public float TargetDamage { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerShootingFirearmEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player shooting the firearm.</param>
    /// <param name="firearm">The firearm which is being shot.</param>
    /// <param name="target">The target which is getting hit.</param>
    /// <param name="damage">The damage to deal to the target.</param>
    /// <param name="raycastHit">The raycast which was used.</param>
    /// <param name="targetPlayer">The player which was hit.</param>
    public PlayerShotFirearmEventArgs(ExPlayer player, Firearm firearm, IDestructible target, float damage, RaycastHit raycastHit, ExPlayer? targetPlayer)
    {
        Player = player;
        Firearm = firearm;
        RaycastHit = raycastHit;
        
        Target = target;
        TargetDamage = damage;
        TargetPlayer = targetPlayer;
    }
}