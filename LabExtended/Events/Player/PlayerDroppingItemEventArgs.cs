using InventorySystem.Items;

using LabApi.Features.Wrappers;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries to drop an item.
    /// </summary>
    public class PlayerDroppingItemEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the player who's dropping the item.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the item to be dropped.
        /// </summary>
        public Item Item { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the item should be thrown.
        /// </summary>
        public bool IsThrow { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerDroppingItemEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player dropping an item.</param>
        /// <param name="item">The dropped item.</param>
        /// <param name="isThrow">Is the drop a throw request?</param>
        public PlayerDroppingItemEventArgs(ExPlayer player, ItemBase item, bool isThrow)
        {
            Player = player;
            IsThrow = isThrow;

            Item = Item.Get(item);
        }
    }
}