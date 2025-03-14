using LabExtended.API;
using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Entries;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a new settings entry instance is created.
    /// </summary>
    public class PlayerSettingsEntryCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The entry that was created.
        /// </summary>
        public SettingsEntry Entry { get; }
        
        /// <summary>
        /// The menu this entry belongs to (null if none).
        /// </summary>
        public SettingsMenu? Menu { get; }
        
        /// <summary>
        /// Gets the player this entry is for.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerSettingsEntryCreatedEventArgs"/> instance.
        /// </summary>
        /// <param name="entry">The created entry.</param>
        /// <param name="menu">Menu of the entry.</param>
        /// <param name="player">Target player.</param>
        public PlayerSettingsEntryCreatedEventArgs(SettingsEntry entry, SettingsMenu menu, ExPlayer player)
        {
            Entry = entry;
            Menu = menu;
            Player = player;
        }
    }
}