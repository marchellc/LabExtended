using InventorySystem.Items;

using LabApi.Features.Wrappers;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called after a player selects a new item to hold.
    /// </summary>
    public class PlayerSelectedItemEventArgs : EventArgs
    {
        /// <summary>
        /// The player who selected a new item.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the previous item instance.
        /// </summary>
        public Item? PreviousItem { get; }

        /// <summary>
        /// Gets the new item instance.
        /// </summary>
        public Item? NewItem { get; }

        /// <summary>
        /// Gets the previous item identifier.
        /// </summary>
        public ItemIdentifier PreviousIdentifier { get; }

        /// <summary>
        /// Gets the new item identifier.
        /// </summary>
        public ItemIdentifier NewIdentifier { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerSelectedItemEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player who switched their held item.</param>
        /// <param name="previousItem">The previously held item.</param>
        /// <param name="newItem">The newly held item.</param>
        /// <param name="previousIdentifier">Identifier of the previously held item.</param>
        /// <param name="newIdentifier">Identifier of the newly held item.</param>
        public PlayerSelectedItemEventArgs(ExPlayer player, ItemBase? previousItem, ItemBase? newItem,
            ItemIdentifier previousIdentifier, ItemIdentifier newIdentifier)
        {
            Player = player;

            PreviousItem = previousItem != null ? Item.Get(previousItem) : null;
            PreviousIdentifier = previousIdentifier;

            NewItem = newItem != null ? Item.Get(newItem) : null;
            NewIdentifier = newIdentifier;
        }
    }
}