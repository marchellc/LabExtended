using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Menus;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Settings;

/// <summary>
/// Used to build menus when a player joins.
/// </summary>
public class SettingsBuilder
{
    private List<Func<SettingsEntry>> _entryBuilders = new();
    private List<Func<SettingsMenu>> _menuBuilders = new();

    /// <summary>
    /// Gets or sets the predicate required to match.
    /// </summary>
    public Predicate<ExPlayer> Predicate { get; set; }

    /// <summary>
    /// Gets or sets the method used to build settings.
    /// </summary>
    public Action<List<SettingsEntry>> SettingsBuilders { get; set; }
    
    /// <summary>
    /// Gets or sets the method used to build menus.
    /// </summary>
    public Action<List<SettingsMenu>> MenuBuilders { get; set; }

    /// <summary>
    /// Gets the custom ID of this builder.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// Creates a new <see cref="SettingsBuilder"/> instance.
    /// </summary>
    /// <param name="customId">The custom ID of the builder.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SettingsBuilder(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId))
            throw new ArgumentNullException(nameof(customId));

        SettingsBuilders = BuildEntries;
        MenuBuilders = BuildMenus;

        CustomId = customId;
    }

    /// <summary>
    /// Adds a menu to the builder.
    /// </summary>
    /// <typeparam name="T">The menu type.</typeparam>
    /// <returns>This builder instance.</returns>
    public SettingsBuilder WithMenu<T>() where T : SettingsMenu, new()
    {
        _menuBuilders.Add(() => Activator.CreateInstance<T>());
        return this;
    }

    /// <summary>
    /// Adds a menu builder to the builder.
    /// </summary>
    /// <param name="menuBuilder">The method used to construct the menu.</param>
    /// <returns>This builder instance.</returns>
    public SettingsBuilder WithMenu(Func<SettingsMenu> menuBuilder)
    {
        if (menuBuilder is null)
            throw new ArgumentNullException(nameof(menuBuilder));

        _menuBuilders.Add(menuBuilder);
        return this;
    }

    /// <summary>
    /// Adds an entry to the builder.
    /// </summary>
    /// <param name="entryBuilder">The method used to construct the entry.</param>
    /// <returns>This builder instance.</returns>
    public SettingsBuilder WithEntry(Func<SettingsEntry> entryBuilder)
    {
        if (entryBuilder is null)
            throw new ArgumentNullException(nameof(entryBuilder));

        _entryBuilders.Add(entryBuilder);
        return this;
    }

    /// <summary>
    /// Sets the builder's predicate.
    /// </summary>
    /// <param name="predicate">The new predicate.</param>
    /// <returns>This builder instance.</returns>
    public SettingsBuilder WithPredicate(Predicate<ExPlayer> predicate)
    {
        Predicate = predicate;
        return this;
    }

    private void BuildMenus(List<SettingsMenu> menus)
    {
        for (var i = 0; i < _menuBuilders.Count; i++)
        {
            menus.Add(_menuBuilders[i]());
        }
    }

    private void BuildEntries(List<SettingsEntry> entries)
    {
        for (var i = 0; i < _entryBuilders.Count; i++)
        {
            entries.Add(_entryBuilders[i]());
        }
    }
}