using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;

namespace LabExtended.API.Settings
{
    public class SettingsBuilder
    {
        public Predicate<ExPlayer> Predicate { get; set; }

        public Action<List<SettingsEntry>> SettingsBuilders { get; set; }
        public Action<List<SettingsMenu>> MenuBuilders { get; set; }

        public string CustomId { get; set; }

        public SettingsBuilder(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));
            
            CustomId = customId;
        }
    }
}