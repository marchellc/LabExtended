using InventorySystem.Items.Firearms;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Firearms;

/// <summary>
/// Called when a ray cast is performed for a firearm shot.
/// </summary>
public class FirearmRayCastEventArgs : BooleanEventArgs
{
    /// <summary>
    /// The player who shot the firearm.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// The firearm which is being shot.
    /// </summary>
    public Firearm Firearm { get; }
    
    /// <summary>
    /// Gets the maximum raycast distance.
    /// </summary>
    public float MaxDistance { get; }
    
    /// <summary>
    /// Gets the source ray.
    /// </summary>
    public Ray Ray { get; }
    
    /// <summary>
    /// Gets or sets the raycast's hit.
    /// </summary>
    public RaycastHit? Hit { get; set; }
    
    /// <summary>
    /// Creates a new <see cref="FirearmRayCastEventArgs"/> event.
    /// </summary>
    /// <param name="player">The player who is shooting the firearm.</param>
    /// <param name="firearm">The firearm which is being shot.</param>
    /// <param name="ray">The source hit ray.</param>
    /// <param name="maxDistance">The maximum ray distance.</param>
    /// <param name="hit">The calculated raycast hit (or null).</param>
    public FirearmRayCastEventArgs(ExPlayer player, Firearm firearm, Ray ray, float maxDistance, RaycastHit? hit)
    {
        Player = player;
        Firearm = firearm;
        Ray = ray;
        MaxDistance = maxDistance;
        Hit = hit;
    }
}