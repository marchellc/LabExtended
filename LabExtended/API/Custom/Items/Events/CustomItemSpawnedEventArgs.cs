using InventorySystem.Items.Pickups;

namespace LabExtended.API.Custom.Items.Events
{
    /// <summary>
    /// Gets called when a custom item is spawned.
    /// </summary>
    public class CustomItemSpawnedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the custom item that was spawned.
        /// </summary>
        public CustomItem CustomItem { get; }

        /// <summary>
        /// Gets the pickup that was spawned.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Gets or sets the data associated with the spawned pickup.
        /// </summary>
        public object? PickupData { get; set; }

        /// <summary>
        /// Initializes a new instance of the CustomItemSpawnedEventArgs class with the specified custom item, pickup
        /// object, and optional pickup data.
        /// </summary>
        /// <param name="item">The custom item that was spawned. Cannot be null.</param>
        /// <param name="pickup">The pickup object associated with the spawned item. Cannot be null.</param>
        /// <param name="pickupData">Optional data related to the pickup. May be null if no additional data is provided.</param>
        public CustomItemSpawnedEventArgs(CustomItem item, ItemPickupBase pickup, object? pickupData)
        {
            CustomItem = item;

            Pickup = pickup;
            PickupData = pickupData;
        }
    }
}