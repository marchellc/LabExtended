using LabExtended.API.Settings.Entries;

namespace LabExtended.Events.Settings
{
    public class SettingsEntryCreatedArgs
    {
        public SettingsEntry Entry { get; }

        public SettingsEntryCreatedArgs(SettingsEntry entry)
            => Entry = entry;
    }
}