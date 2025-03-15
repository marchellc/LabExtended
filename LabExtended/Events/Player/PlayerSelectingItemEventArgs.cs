using LabApi.Features.Wrappers;

using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player tries changing their currently held item.
    /// </summary>
    public class PlayerSelectingItemEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the player who's attempting to switch their item.
        /// </summary>
        public ExPlayer? Player { get; }

        /// <summary>
        /// Gets the currently selected item.
        /// </summary>
        public Item CurrentItem { get; }

        /// <summary>
        /// Gets or sets the new item.
        /// </summary>
        public Item NextItem { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerSelectingItemEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player switching their held item.</param>
        /// <param name="current">The currently held item.</param>
        /// <param name="next">The item that is going to be held.</param>
        public PlayerSelectingItemEventArgs(ExPlayer player, Item current, Item next)
        {
            Player = player;
            NextItem = next;
            CurrentItem = current;
        }
    }
}