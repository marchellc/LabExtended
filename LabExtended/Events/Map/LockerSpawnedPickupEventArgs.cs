using InventorySystem.Items.Pickups;

using MapGeneration.Distributors;

namespace LabExtended.Events.Map;

/// <summary>
/// Gets triggered when a locker is spawning pickup in one of it's chambers.
/// </summary>
public class LockerSpawnedPickupEventArgs : EventArgs
{
    /// <summary>
    /// Gets the locker that contains the target chamber.
    /// </summary>
    public Locker Locker { get; }

    /// <summary>
    /// Gets the target chamber.
    /// </summary>
    public LockerChamber Chamber { get; }
    
    /// <summary>
    /// Gets the spawned pickup instance.
    /// </summary>
    public ItemPickupBase Pickup { get; }

    /// <summary>
    /// Creates a new <see cref="LockerSpawnedPickupEventArgs"/> instance.
    /// </summary>
    /// <param name="locker">The locker that this chamber belongs to.</param>
    /// <param name="chamber">The chamber that is being filled.</param>
    public LockerSpawnedPickupEventArgs(Locker locker, LockerChamber chamber, ItemPickupBase pickup)
    {
        Locker = locker;
        Chamber = chamber;
        Pickup = pickup;
    }
}