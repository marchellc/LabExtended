using LabExtended.API.Settings.Entries;

namespace LabExtended.Events.Player.Settings
{
    /// <summary>
    /// Gets called when the server receives an entry update.
    /// </summary>
    public class PlayerSettingsEntryUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The updated entry.
        /// </summary>
        public SettingsEntry Entry { get; }

        /// <summary>
        /// Creates a new <see cref="PlayerSettingsEntryUpdatedEventArgs"/> instance.
        /// </summary>
        /// <param name="entry">The updated entry.</param>
        public PlayerSettingsEntryUpdatedEventArgs(SettingsEntry entry)
            => Entry = entry;
    }
}