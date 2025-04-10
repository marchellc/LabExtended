using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API;

namespace LabExtended.Utilities;

/// <summary>
/// Used to track item serials.
/// </summary>
public class ItemTracker : IDisposable
{
    /// <summary>
    /// Gets a list of item trackers.
    /// </summary>
    public static Dictionary<ushort, ItemTracker> Trackers { get; } = new();
    
    /// <summary>
    /// Gets the tracked item serial.
    /// </summary>
    public ushort ItemSerial { get; }
    
    /// <summary>
    /// Gets the player that currently owns the item.
    /// </summary>
    public ExPlayer? Owner { get; internal set; }
    
    /// <summary>
    /// Gets the inventory item instance.
    /// </summary>
    public ItemBase? Item { get; internal set; }
    
    /// <summary>
    /// Gets the dropped item instance.
    /// </summary>
    public ItemPickupBase? Pickup { get; internal set; }

    /// <summary>
    /// Creates a new <see cref="ItemTracker"/> instance.
    /// </summary>
    /// <param name="item">The target inventory item.</param>
    public ItemTracker(ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        Item = item;
        ItemSerial = item.ItemSerial;
    }
    
    /// <summary>
    /// Creates a new <see cref="ItemTracker"/> instance.
    /// </summary>
    /// <param name="item">The target dropped item.</param>
    public ItemTracker(ItemPickupBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        Pickup = item;
        ItemSerial = item.Info.Serial;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        
    }
}