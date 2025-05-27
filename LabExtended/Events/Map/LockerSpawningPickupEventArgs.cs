using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Events.Map;

/// <summary>
/// Gets triggered when a locker is spawning pickup in one of it's chambers.
/// </summary>
public class LockerSpawningPickupEventArgs : BooleanEventArgs
{
    /// <summary>
    /// The locker that contains the target chamber.
    /// </summary>
    public Locker Locker { get; }

    /// <summary>
    /// The target chamber.
    /// </summary>
    public LockerChamber Chamber { get; }

    /// <summary>
    /// The type of the item to spawn.
    /// </summary>
    public ItemType Type { get; set; }

    /// <summary>
    /// Gets or sets the item's position.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the item's rotation.
    /// </summary>
    public Quaternion Rotation { get; set; }

    /// <summary>
    /// Creates a new <see cref="LockerSpawningPickupEventArgs"/> instance.
    /// </summary>
    /// <param name="locker">The locker that this chamber belongs to.</param>
    /// <param name="chamber">The chamber that is being filled.</param>
    /// <param name="type">The type of item that should be spawned.</param>
    /// <param name="position">Position of the item's spawn point.</param>
    /// <param name="rotation">Rotation of the item's spawn point.</param>
    public LockerSpawningPickupEventArgs(Locker locker, LockerChamber chamber, ItemType type, Vector3 position,
        Quaternion rotation)
    {
        Locker = locker;
        Chamber = chamber;
        Type = type;
        Position = position;
        Rotation = rotation;
    }
}