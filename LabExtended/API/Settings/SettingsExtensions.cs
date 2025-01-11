using LabExtended.API.Settings.Entries;

namespace LabExtended.API.Settings;

public static class SettingsExtensions
{
    public static List<SettingsEntry> WithEntry(this List<SettingsEntry> entries, SettingsEntry entry)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        entries.Add(entry);
        return entries;
    }

    public static List<SettingsEntry> WithEntries(this List<SettingsEntry> entries, params SettingsEntry[] settingsEntries)
    {
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

        entries.AddRange(settingsEntries);
        return entries;
    }
}