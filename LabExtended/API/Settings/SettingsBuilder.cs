using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;

namespace LabExtended.API.Settings
{
    public class SettingsBuilder
    {
        private List<Func<SettingsEntry>> _entryBuilders = new List<Func<SettingsEntry>>();
        private List<Func<SettingsMenu>> _menuBuilders = new List<Func<SettingsMenu>>();

        public Predicate<ExPlayer> Predicate { get; set; }

        public Action<List<SettingsEntry>> SettingsBuilders { get; set; }
        public Action<List<SettingsMenu>> MenuBuilders { get; set; }

        public string CustomId { get; }

        public SettingsBuilder(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            SettingsBuilders = BuildEntries;
            MenuBuilders = BuildMenus;
            
            CustomId = customId;
        }

        public SettingsBuilder WithMenu<T>() where T : SettingsMenu, new()
        {
            _menuBuilders.Add(() => Activator.CreateInstance<T>());
            return this;
        }

        public SettingsBuilder WithMenu(Func<SettingsMenu> menuBuilder)
        {
            if (menuBuilder is null)
                throw new ArgumentNullException(nameof(menuBuilder));

            _menuBuilders.Add(menuBuilder);
            return this;
        }

        public SettingsBuilder WithEntry(Func<SettingsEntry> entryBuilder)
        {
            if (entryBuilder is null)
                throw new ArgumentNullException(nameof(entryBuilder));

            _entryBuilders.Add(entryBuilder);
            return this;
        }

        public SettingsBuilder WithPredicate(Predicate<ExPlayer> predicate)
        {
            Predicate = predicate;
            return this;
        }

        private void BuildMenus(List<SettingsMenu> menus)
        {
            for (int i = 0; i < _menuBuilders.Count; i++)
                menus.Add(_menuBuilders[i]());
        }

        private void BuildEntries(List<SettingsEntry> entries)
        {
            for (int i = 0; i < _entryBuilders.Count; i++)
                entries.Add(_entryBuilders[i]());
        }
    }
}