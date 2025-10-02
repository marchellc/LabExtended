using InventorySystem.Items;
using InventorySystem.Items.Pickups;

namespace LabExtended.API.Custom.Items.Events
{
    /// <summary>
    /// Gets called when a custom item is dropped.
    /// </summary>
    public class CustomItemDroppedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the custom item that was dropped.
        /// </summary>
        public CustomItem CustomItem { get; }

        /// <summary>
        /// Gets the player who dropped the item.
        /// </summary>
        public ExPlayer Owner { get; }

        /// <summary>
        /// Gets the instance of the inventory item that was dropped.
        /// </summary>
        public ItemBase Item { get; }

        /// <summary>
        /// Gets the spawned pickup.
        /// </summary>
        public ItemPickupBase Pickup { get; }

        /// <summary>
        /// Gets or sets the data to be associated with the pickup.
        /// </summary>
        public object? PickupData { get; set; }

        /// <summary>
        /// Whether or not the item was thrown.
        /// </summary>
        public bool WasThrow { get; }

        /// <summary>
        /// Initializes a new instance of the CustomItemDroppedEventArgs class with the specified custom item, owner,
        /// item, pickup, pickup data, and throw status.
        /// </summary>
        /// <param name="customItem">The custom item associated with the drop event. Cannot be null.</param>
        /// <param name="owner">The player who owned the item at the time it was dropped. Cannot be null.</param>
        /// <param name="item">The base item that was dropped. Cannot be null.</param>
        /// <param name="pickup">The pickup object representing the dropped item in the game world. Cannot be null.</param>
        /// <param name="pickupData">Additional data related to the pickup, or null if no extra data is provided.</param>
        /// <param name="wasThrow">Indicates whether the item was dropped as a result of being thrown. Set to <see langword="true"/> if the
        /// item was thrown; otherwise, <see langword="false"/>.</param>
        public CustomItemDroppedEventArgs(CustomItem customItem, ExPlayer owner, ItemBase item, ItemPickupBase pickup, object? pickupData, bool wasThrow)
        {
            CustomItem = customItem;

            Owner = owner;
            Item = item;

            Pickup = pickup;
            PickupData = pickupData;

            WasThrow = wasThrow;
        }
    }
}