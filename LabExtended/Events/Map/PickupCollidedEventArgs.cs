using InventorySystem.Items.Pickups;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Map;

/// <summary>
/// Gets called when a <see cref="CollisionDetectionPickup"/> collides with something.
/// </summary>
public class PickupCollidedEventArgs : BooleanEventArgs
{
    /// <summary>
    /// Gets the owner of the pickup.
    /// </summary>
    public ExPlayer? Player { get; }
    
    /// <summary>
    /// Gets the pickup that collided.
    /// </summary>
    public CollisionDetectionPickup Pickup { get; }
    
    /// <summary>
    /// Gets the information about the collision.
    /// </summary>
    public Collision Collision { get; }

    /// <summary>
    /// Creates a new <see cref="PickupCollidedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The pickup owner.</param>
    /// <param name="pickup">The colliding pickup.</param>
    /// <param name="collision">The collision data.</param>
    public PickupCollidedEventArgs(ExPlayer? player, CollisionDetectionPickup pickup, Collision collision)
    {
        Player = player;
        Pickup = pickup;
        Collision = collision;
    }
}