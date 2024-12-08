using LabExtended.API.Settings.Entries;

namespace LabExtended.Events.Settings
{
    public class SettingsEntryUpdatedArgs
    {
        public SettingsEntry Entry { get; }

        public SettingsEntryUpdatedArgs(SettingsEntry entry)
            => Entry = entry;
    }
}