using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Custom.Items.Enums;

namespace LabExtended.API.Custom.Items.Events
{
    /// <summary>
    /// Gets called when a custom item is added to a player's inventory.
    /// </summary>
    public class CustomItemAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player who received the item.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the custom item that was added.
        /// </summary>
        public CustomItem CustomItem { get; }

        /// <summary>
        /// Gets the reason why the item was added.
        /// </summary>
        public CustomItemAddReason AddReason { get; }

        /// <summary>
        /// Gets the instance of the added inventory item.
        /// </summary>
        public ItemBase AddedItem { get; }

        /// <summary>
        /// Gets or sets the data of the added item.
        /// </summary>
        public object? AddedData { get; set; }

        /// <summary>
        /// Gets the picked the item originates from (only if <see cref="AddReason"/> is <see cref="CustomItemAddReason.PickedUp"/>)."/>
        /// </summary>
        public ItemPickupBase? OriginalPickup { get; }

        /// <summary>
        /// Gets the data of the original pickup.
        /// </summary>
        public object? OriginalData { get; }

        /// <summary>
        /// Initializes a new instance of the CustomItemAddedEventArgs class, providing information about a custom item
        /// that was added to a player.
        /// </summary>
        /// <param name="player">The player to whom the custom item was added. Cannot be null.</param>
        /// <param name="customItem">The custom item that was added. Cannot be null.</param>
        /// <param name="addReason">The reason for which the custom item was added to the player.</param>
        /// <param name="addedItem">The underlying item base instance representing the added item. Cannot be null.</param>
        /// <param name="addedData">Optional data associated with the added item. May be null if no additional data is provided.</param>
        /// <param name="originalPickup">The original item pickup base, if the custom item was added via a pickup. May be null if not applicable.</param>
        /// <param name="originalData">Optional data associated with the original pickup. May be null if no additional data is available.</param>
        public CustomItemAddedEventArgs(ExPlayer player, CustomItem customItem, CustomItemAddReason addReason, 
            ItemBase addedItem, object? addedData, ItemPickupBase? originalPickup, object? originalData)
        {
            Player = player;
            CustomItem = customItem;
            AddReason = addReason;
            AddedItem = addedItem;
            AddedData = addedData;
            OriginalPickup = originalPickup;
            OriginalData = originalData;
        }
    }
}