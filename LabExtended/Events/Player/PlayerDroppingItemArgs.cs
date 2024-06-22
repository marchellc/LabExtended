using InventorySystem.Items;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries to drop an item.
    /// </summary>
    public class PlayerDroppingItemArgs : HookBooleanCancellableEventBase
    {
        /// <summary>
        /// Gets the player who's dropping the item.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the item to be dropped.
        /// </summary>
        public ItemBase Item { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the item should be thrown.
        /// </summary>
        public bool IsThrow { get; set; }

        internal PlayerDroppingItemArgs(ExPlayer player, ItemBase item, bool isThrow)
        {
            Player = player;
            Item = item;
            IsThrow = isThrow;
        }
    }
}