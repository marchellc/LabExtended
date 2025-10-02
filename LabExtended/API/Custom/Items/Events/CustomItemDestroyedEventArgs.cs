using InventorySystem.Items;
using InventorySystem.Items.Pickups;

namespace LabExtended.API.Custom.Items.Events
{
    /// <summary>
    /// Gets called when a custom item is destroyed.
    /// </summary>
    public class CustomItemDestroyedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the custom item that was destroyed.
        /// </summary>
        public CustomItem CustomItem { get; }

        /// <summary>
        /// Gets the serial of the tracked item instance.
        /// </summary>
        public ushort TrackedSerial { get; }

        /// <summary>
        /// Gets the instance of the item that was destroyed, if any (<see cref="Pickup"/> will not be null if this is null).
        /// </summary>
        public ItemBase? Item { get; }

        /// <summary>
        /// Gets the instance of the pickup that was destroyed, if any (<see cref="Item"/> will not be null if this is null).
        /// </summary>
        public ItemPickupBase? Pickup { get; }

        /// <summary>
        /// Gets the data that was associated with the destroyed instance.
        /// </summary>
        public object? Data { get; }

        /// <summary>
        /// Initializes a new instance of the CustomItemDestroyedEventArgs class with the specified custom item, serial
        /// number, item reference, pickup reference, and additional data.
        /// </summary>
        /// <param name="customItem">The custom item associated with the destruction event. Cannot be null.</param>
        /// <param name="trackedSerial">The serial number used to track the destroyed custom item.</param>
        /// <param name="item">The item instance that was destroyed, or null if not applicable.</param>
        /// <param name="pickup">The pickup instance related to the destroyed item, or null if not applicable.</param>
        /// <param name="data">Additional data associated with the destruction event. May be null if no extra data is provided.</param>
        public CustomItemDestroyedEventArgs(CustomItem customItem, ushort trackedSerial, ItemBase? item, ItemPickupBase? pickup, object? data)
        {
            CustomItem = customItem;
            TrackedSerial = trackedSerial;
            Item = item;
            Pickup = pickup;
            Data = data;
        }
    }
}
