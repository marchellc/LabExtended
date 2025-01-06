using LabExtended.API;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;

namespace LabExtended.Events.Settings
{
    public class SettingsEntryCreatedArgs
    {
        public SettingsEntry Entry { get; }
        public SettingsMenu Menu { get; }
        
        public ExPlayer Player { get; }

        public SettingsEntryCreatedArgs(SettingsEntry entry, SettingsMenu menu, ExPlayer player)
        {
            Entry = entry;
            Menu = menu;
            Player = player;
        }
    }
}