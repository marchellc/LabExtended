using InventorySystem.Items;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called after a player selects a new item to hold.
    /// </summary>
    public class PlayerSelectedItemArgs
    {
        /// <summary>
        /// The player who selected a new item.
        /// </summary>
        public ExPlayer? Player { get; }

        /// <summary>
        /// Gets the previous item instance.
        /// </summary>
        public ItemBase PreviousItem { get; }

        /// <summary>
        /// Gets the new item instance.
        /// </summary>
        public ItemBase NewItem { get; }

        /// <summary>
        /// Gets the previous item identifier.
        /// </summary>
        public ItemIdentifier PreviousIdentifier { get; }

        /// <summary>
        /// Gets the new item identifier.
        /// </summary>
        public ItemIdentifier NewIdentifier { get; }

        internal PlayerSelectedItemArgs(ExPlayer? player, ItemBase previousItem, ItemBase newItem, ItemIdentifier previousIdentifier, ItemIdentifier newIdentifier)
        {
            Player = player;

            PreviousItem = previousItem;
            PreviousIdentifier = previousIdentifier;

            NewItem = newItem;
            NewIdentifier = newIdentifier;
        }
    }
}