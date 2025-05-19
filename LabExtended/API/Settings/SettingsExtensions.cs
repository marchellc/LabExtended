using LabExtended.API.Settings.Entries;

namespace LabExtended.API.Settings;

/// <summary>
/// Extensions targeting the Settings API.
/// </summary>
public static class SettingsExtensions
{
    /// <summary>
    /// Adds an entry instance to the list.
    /// </summary>
    /// <param name="entries">The target list.</param>
    /// <typeparam name="T">The entry type to add.</typeparam>
    /// <returns>The target list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<SettingsEntry> WithEntry<T>(this List<SettingsEntry> entries) where T : SettingsEntry, new()
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        entries.Add(Activator.CreateInstance<T>());
        return entries;
    }
    
    /// <summary>
    /// Adds an entry instance to the list.
    /// </summary>
    /// <param name="entries">The target list.</param>
    /// <param name="entry">The entry instance.</param>
    /// <returns>The target list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<SettingsEntry> WithEntry(this List<SettingsEntry> entries, SettingsEntry entry)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        entries.Add(entry);
        return entries;
    }

    /// <summary>
    /// Adds multiple entries to the list.
    /// </summary>
    /// <param name="entries">The target list.</param>
    /// <param name="settingsEntries">The entries to add.</param>
    /// <returns>The target list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<SettingsEntry> WithEntries(this List<SettingsEntry> entries, params SettingsEntry[] settingsEntries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        entries.AddRange(settingsEntries);
        return entries;
    }
}