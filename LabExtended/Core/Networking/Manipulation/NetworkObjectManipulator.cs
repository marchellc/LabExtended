using AdminToys;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Core.Networking.Manipulation.Wrappers;
using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Core.Networking.Manipulation;

/// <summary>
/// Used to move or rotate network objects.
/// </summary>
public class NetworkObjectManipulator : IDisposable
{
    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    public INetworkObject? Target { get; set; }
    
    /// <summary>
    /// Gets or sets the object's target to follow.
    /// </summary>
    public INetworkObject? Follow { get; set; }

    /// <summary>
    /// Gets the manipulator's following settings.
    /// </summary>
    public NetworkObjectFollowSettings FollowSettings { get; } = new();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Target?.ResetObject();
        Target = null;
        
        Follow = null;
    }

    /// <summary>
    /// Updates the object's positions.
    /// </summary>
    public void Update()
    {
        if (Target is { IsAlive: true })
        {
            if (Follow is { IsFollowable: true })
            {
                var distance = Vector3.Distance(Target.Position, Follow.Position);

                if (distance > FollowSettings.MaxDistance)
                {
                    Target.Position = Follow.Position;
                    return;
                }
                
                if (distance > FollowSettings.MinDistance)
                {
                    var direction = Follow.Position - Target.Position;
                    var move = Time.deltaTime * FollowSettings.Speed * direction.normalized;

                    Target.Position += move;
                    Target.LookAtDirection(direction);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to get a networked object from a game object.
    /// </summary>
    /// <param name="gameObject">The game object.</param>
    /// <param name="manipulator">The created manipulator.</param>
    /// <returns>true if a manipulator was created</returns>
    public static bool FromObject(GameObject gameObject, out NetworkObjectManipulator? manipulator)
    {
        manipulator = null;

        if (gameObject == null)
            throw new ArgumentNullException(nameof(gameObject));

        if (gameObject.TryFindComponent<ItemPickupBase>(out var pickup))
        {
            manipulator = new() { Target = new ItemPickupNetworkObject(pickup) };
            return true;
        }

        if (gameObject.TryFindComponent<AdminToyBase>(out var toy))
        {
            manipulator = new() { Target = new AdminToyNetworkObject(AdminToy.Get(toy)) };
            return true;
        }

        if (gameObject.TryFindComponent<ReferenceHub>(out var referenceHub))
        {
            manipulator = new() { Target = new PlayerNetworkObject(ExPlayer.Get(referenceHub)) };
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to get a networked object from a raycast.
    /// </summary>
    /// <param name="hit">The raycast result.</param>
    /// <param name="manipulator">The created manipulator.</param>
    /// <returns>true if a manipulator was created</returns>
    public static bool FromRaycast(RaycastHit hit, out NetworkObjectManipulator? manipulator)
        => FromObject(hit.collider?.gameObject, out manipulator);
}