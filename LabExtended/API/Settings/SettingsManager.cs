using LabApi.Loader;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;
using LabExtended.API.Settings.Interfaces;
using LabExtended.API.Settings.Menus;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Patches.Functions.Settings;
using Mirror;
using NorthwoodLib.Pools;
using System.Reflection;
using UserSettings.ServerSpecific;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Settings;

/// <summary>
/// Manages custom user-specific server settings.
/// </summary>
public static class SettingsManager
{
    /// <summary>
    /// Gets a list of registered setting menu builders.
    /// </summary>
    public static List<SettingsBuilder> AllBuilders { get; } = new();

    /// <summary>
    /// Gets or sets the server-side settings version.
    /// </summary>
    public static int Version
    {
        get => ServerSpecificSettingsSync.Version;
        set => ServerSpecificSettingsSync.Version = value;
    }

    /// <summary>
    /// Synchronizes all registered entries.
    /// </summary>
    /// <param name="player"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SyncEntries(this ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        var list = ListPool<ServerSpecificSettingBase>.Shared.Rent();
        var headers = ListPool<string>.Shared.Rent();

        foreach (var settingsEntry in player.settingsIdLookup)
        {
            if (settingsEntry.Value?.Base is null)
                continue;

            if (!settingsEntry.Value.Player)
                continue;

            if (settingsEntry.Value.IsHidden)
                continue;

            if (settingsEntry.Value.Menu != null)
            {
                if (settingsEntry.Value.Menu.IsHidden)
                    continue;

                if (!string.IsNullOrWhiteSpace(settingsEntry.Value.Menu.Header) &&
                    !headers.Contains(settingsEntry.Value.Menu.CustomId))
                {
                    headers.Add(settingsEntry.Value.Menu.CustomId);
                    list.Add(new SSGroupHeader(settingsEntry.Value.Menu.Header,
                        settingsEntry.Value.Menu.HeaderReducedPadding, settingsEntry.Value.Menu.HeaderHint));
                }
            }

            list.Add(settingsEntry.Value.Base);
        }


        Dictionary<Assembly, ServerSpecificSettingBase[]> playerSettings = DictionaryPool<Assembly, ServerSpecificSettingBase[]>.Shared.Rent();
        playerSettings.AddRange(VanillaSettingsAdapter.SssByAssemblyGlobal);
        if (VanillaSettingsAdapter.SssByAssemblyPersonal.TryGetValue(player, out var loadedPlayerSettings)) {
            foreach (var kvp in loadedPlayerSettings) {
                playerSettings[kvp.Key] = kvp.Value;
            }
        }

        foreach (var sssByAssemblyEntry in playerSettings) {
            Assembly pluginAssembly = sssByAssemblyEntry.Key;
            ServerSpecificSettingBase[] collection = sssByAssemblyEntry.Value;
            if (collection.First() is not SSGroupHeader) {
                string pluginName = PluginLoader.Plugins.TryGetFirst(o => o.Value.Equals(pluginAssembly), out var result) ? result.Key.Name : pluginAssembly.GetName().Name;
                list.Add(new SSGroupHeader(pluginName.SpaceByUpperCase()));
                collection.ForEach(list.Add);
            } else if (collection.Length > 1) {
                collection.ForEach(list.Add);
            }
        }

        player.Send(new SSSEntriesPack(ListPool<ServerSpecificSettingBase>.Shared.ToArrayReturn(list), Version));

        ListPool<string>.Shared.Return(headers);
    }

    /// <summary>
    /// Attempts to retrieve a menu for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="menu">The resolved menu instance.</param>
    /// <typeparam name="TMenu">The type of menu to find.</typeparam>
    /// <returns>true if the menu was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetMenu<TMenu>(this ExPlayer player, out TMenu menu) where TMenu : SettingsMenu
    {
        if (!player)
            throw new ArgumentNullException(nameof(player));

        menu = null;

        foreach (var value in player.settingsMenuLookup)
        {
            if (value.Value is TMenu menuItem)
            {
                menu = menuItem;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve a menu for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="menuId">ID of the menu to find.</param>
    /// <param name="menu">The resolved menu instance.</param>
    /// <returns>true if the menu was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetMenu(this ExPlayer player, string menuId, out SettingsMenu menu)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.settingsMenuLookup is null)
        {
            menu = null;
            return false;
        }
        
        return player.settingsMenuLookup.TryGetValue(menuId, out menu);
    }

    /// <summary>
    /// Attempts to retrieve an entry for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="generatedId">Generated entry ID.</param>
    /// <param name="entry">The resolved entry instance.</param>
    /// <returns>true if the entry was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetEntry(this ExPlayer player, int generatedId, out SettingsEntry entry)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.settingsAssignedIdLookup is null)
        {
            entry = null;
            return false;
        }
        
        return player.settingsAssignedIdLookup.TryGetValue(generatedId, out entry);
    }

    /// <summary>
    /// Attempts to retrieve an entry for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="customId">Custom entry ID.</param>
    /// <param name="entry">The resolved entry instance.</param>
    /// <returns>true if the entry was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetEntry(this ExPlayer player, string customId, out SettingsEntry entry)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.settingsIdLookup is null)
        {
            entry = null;
            return false;
        }
        
        return player.settingsIdLookup.TryGetValue(customId, out entry);
    }

    /// <summary>
    /// Attempts to find an entry matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="entry">The resolved entry instance.</param>
    /// <returns>true if the entry was found</returns>
    public static bool TryGetEntry(Func<SettingsEntry, bool> predicate, out SettingsEntry entry)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < ExPlayer.Players.Count; i++)
        {
            var player = ExPlayer.Players[i];
            
            if (player?.settingsAssignedIdLookup is null)
                continue;

            foreach (var entryPair in player.settingsAssignedIdLookup)
            {
                if (predicate(entryPair.Value))
                {
                    entry = entryPair.Value;
                    return true;
                }
            }
        }
        
        entry = null;
        return false;
    }

    /// <summary>
    /// Gets a menu.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <typeparam name="TMenu">The menu type.</typeparam>
    /// <returns>The resolved menu instance (if found, otherwise null)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TMenu? GetMenu<TMenu>(this ExPlayer player) where TMenu : SettingsMenu
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (player.settingsMenuLookup is null)
            return null;
        
        foreach (var menu in player.settingsMenuLookup)
        {
            if (menu.Value is TMenu value)
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a menu.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="menuId">The ID of the menu.</param>
    /// <returns>The resolved menu instance (if found, otherwise null)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static SettingsMenu? GetMenu(this ExPlayer player, string menuId)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (player.settingsMenuLookup is null)
            return null;
        
        return player.settingsMenuLookup.TryGetValue(menuId, out var menu) ? menu : null;
    }

    /// <summary>
    /// Gets an entry.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="customId">The custom ID of the entry.</param>
    /// <returns>The resolved entry instance (if found, otherwise null)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static SettingsEntry? GetEntry(this ExPlayer player, string customId)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (player.settingsIdLookup is null)
            return null;
        
        return player.settingsIdLookup.TryGetValue(customId, out var entry) ? entry : null;
    }

    /// <summary>
    /// Gets an entry.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="generatedId">The generated ID of the entry.</param>
    /// <returns>The resolved entry instance (if found, otherwise null)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static SettingsEntry? GetEntry(this ExPlayer player, int generatedId)
    { 
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (player.settingsIdLookup is null)
            return null;
        
        return player.settingsAssignedIdLookup.TryGetValue(generatedId, out var entry) ? entry : null;
    }

    /// <summary>
    /// Gets an entry for a given predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The entry instance if found, otherwise null</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static SettingsEntry? GetEntry(Func<SettingsEntry, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        for (var i = 0; i < ExPlayer.Players.Count; i++)
        {
            var player = ExPlayer.Players[i];
            
            if (player?.settingsAssignedIdLookup is null)
                continue;

            foreach (var entryPair in player.settingsAssignedIdLookup)
            {
                if (predicate(entryPair.Value))
                {
                    return entryPair.Value;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Gets a list of entries matching an ID.
    /// </summary>
    /// <param name="generatedId">The generated ID.</param>
    /// <returns>List of matching entries.</returns>
    public static List<SettingsEntry> GetEntries(int generatedId)
        => GetEntries(x => x.AssignedId == generatedId);

    /// <summary>
    /// Gets a list of entries matching an ID.
    /// </summary>
    /// <param name="customId">The custom ID.</param>
    /// <returns>List of matching entries.</returns>
    public static List<SettingsEntry> GetEntries(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId))
            throw new ArgumentNullException(nameof(customId));

        return GetEntries(x => x.CustomId == customId);
    }

    /// <summary>
    /// Gets a list of entries matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>List of matching entries.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<SettingsEntry> GetEntries(Func<SettingsEntry, bool> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var list = new List<SettingsEntry>();

        for (var i = 0; i < ExPlayer.Players.Count; i++)
        {
            var player = ExPlayer.Players[i];
            
            if (player?.settingsAssignedIdLookup is null)
                continue;

            foreach (var entryPair in player.settingsAssignedIdLookup)
            {
                if (predicate(entryPair.Value))
                {
                    list.Add(entryPair.Value);
                }
            }
        }
        
        return list;
    }
    
    /// <summary>
    /// Gets a list of all entries for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>List of all created entries.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<SettingsEntry> GetEntries(this ExPlayer player)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        return player.settingsIdLookup.Values;
    }

    /// <summary>
    /// Adds a new settings builder.
    /// </summary>
    /// <param name="settingsBuilder">The builder to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AddBuilder(SettingsBuilder settingsBuilder)
    {
        if (settingsBuilder is null)
            throw new ArgumentNullException(nameof(settingsBuilder));

        AllBuilders.Add(settingsBuilder);
    }

    /// <summary>
    /// Removes a settings builder.
    /// </summary>
    /// <param name="builderId">ID of the builder.</param>
    /// <returns>true if the builder was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveBuilder(string builderId)
    {
        if (string.IsNullOrWhiteSpace(builderId))
            throw new ArgumentNullException(nameof(builderId));

        return AllBuilders.RemoveAll(x => !string.IsNullOrWhiteSpace(x.CustomId) && x.CustomId == builderId) > 0;
    }

    /// <summary>
    /// Whether or not a specific builder exists.
    /// </summary>
    /// <param name="builderId">The ID of the builder.</param>
    /// <returns>true if the builder exists</returns>
    public static bool HasBuilder(string builderId)
    {
        if (string.IsNullOrWhiteSpace(builderId))
            return false;

        return AllBuilders.Any(x => !string.IsNullOrWhiteSpace(x.CustomId) && x.CustomId == builderId);
    }

    /// <summary>
    /// Adds a new menu to the player.
    /// </summary>
    /// <param name="player">The player to add the menu to.</param>
    /// <param name="menu">The menu to add.</param>
    /// <returns>true if the menu was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddMenu(this ExPlayer player, SettingsMenu menu)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (menu is null)
            throw new ArgumentNullException(nameof(menu));
        
        if (player.settingsMenuLookup is null)
            return false;

        if (player.settingsMenuLookup.ContainsKey(menu.CustomId))
            return false;

        var entries = ListPool<SettingsEntry>.Shared.Rent();
        var curCount = player.settingsIdLookup.Count;

        menu.Player = player;
        menu.BuildMenu(entries);

        menu.Entries = ListPool<SettingsEntry>.Shared.ToArrayReturn(entries);

        player.settingsMenuLookup.Add(menu.CustomId, menu);

        if (!string.IsNullOrEmpty(menu.Header))
        {
            var menuHeader = new SettingsGroup(menu.Header, menu.HeaderReducedPadding, menu.HeaderHint);

            menuHeader.Menu = menu;
            menuHeader.Player = player;

            player.settingsIdLookup.Add(menuHeader.CustomId, menuHeader);
            player.settingsAssignedIdLookup.Add(menuHeader.AssignedId, menuHeader);
        }

        for (var y = 0; y < menu.Entries.Length; y++)
        {
            var menuSetting = menu.Entries[y];

            if (menuSetting != null)
            {
                menuSetting.Player = player;
                menuSetting.Menu = menu;

                player.settingsIdLookup.Add(menuSetting.CustomId, menuSetting);
                player.settingsAssignedIdLookup.Add(menuSetting.AssignedId, menuSetting);

                ExPlayerEvents.OnSettingsEntryCreated(new(menuSetting, menu, player));
            }
        }

        if (curCount != player.settingsIdLookup.Count)
            SyncEntries(player);

        return true;
    }

    /// <summary>
    /// Adds a setting entry for a player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="entry">The entry.</param>
    /// <returns>true if the entry was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static bool AddSetting(ExPlayer player, SettingsEntry entry)
    {
        if (!player)
            throw new ArgumentNullException(nameof(player));

        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        if (player.settingsIdLookup is null || player.settingsAssignedIdLookup is null)
            return false;

        if (player.settingsIdLookup.ContainsKey(entry.CustomId))
            return false;

        if (player.settingsAssignedIdLookup.ContainsKey(entry.AssignedId))
            return false;

        entry.Player = player;

        player.settingsIdLookup.Add(entry.CustomId, entry);
        player.settingsAssignedIdLookup.Add(entry.AssignedId, entry);

        SyncEntries(player);
        return true;
    }

    /// <summary>
    /// Adds a list of entries with a given header.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="entries">The list of entries to add.</param>
    /// <param name="groupHeader">The group header.</param>
    /// <param name="reducedHeaderPadding">Reduced header padding.</param>
    /// <param name="headerHint">Hint displayed when hovering over the header.</param>
    /// <returns>true if the entries were added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddSettings(ExPlayer player, IEnumerable<SettingsEntry> entries, string groupHeader = null,
        bool reducedHeaderPadding = false, string headerHint = null)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));

        if (entries is null)
            throw new ArgumentNullException(nameof(entries));
        
        if (player.settingsIdLookup is null || player.settingsAssignedIdLookup is null)
            return false;

        if (!string.IsNullOrWhiteSpace(groupHeader))
        {
            var header = new SettingsGroup(groupHeader, reducedHeaderPadding, headerHint);

            player.settingsIdLookup[header.CustomId] = header;
            player.settingsAssignedIdLookup[header.AssignedId] = header;
        }

        foreach (var entry in entries)
        {
            entry.Player = player;

            player.settingsIdLookup.Add(entry.CustomId, entry);
            player.settingsAssignedIdLookup.Add(entry.AssignedId, entry);
        }

        SyncEntries(player);
        return true;
    }

    /// <summary>
    /// Whether or not a player has their settings tab open.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns></returns>
    public static bool HasSettingsOpen(this ExPlayer player)
    {
        if (player?.SettingsReport == null)
            return false;

        return player.SettingsReport.Value.TabOpen;
    }

    /// <summary>
    /// Gets the reported user settings version for a player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>0 if the player has not reported their settings version, otherwise the reported version</returns>
    public static int GetUserVersion(this ExPlayer player)
    {
        if (player?.SettingsReport == null)
            return 0;

        return player.SettingsReport.Value.Version;
    }

    /// <summary>
    /// Gets a stable hash code of a string.
    /// </summary>
    /// <param name="customId">The string.</param>
    /// <returns>Stable hash code.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static int GetIntegerId(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId))
            throw new ArgumentNullException(nameof(customId));

        return customId.GetStableHashCode();
    }

    private static void OnPlayerVerified(ExPlayer player)
    {
        try
        {
            if (!player)
                return;

            for (var i = 0; i < AllBuilders.Count; i++)
            {
                try
                {
                    var builder = AllBuilders[i];

                    if (builder.Predicate != null && !builder.Predicate(player))
                        continue;

                    var builtSettings = ListPool<SettingsEntry>.Shared.Rent();
                    var builtMenus = ListPool<SettingsMenu>.Shared.Rent();

                    builder.SettingsBuilders.InvokeSafe(builtSettings);
                    builder.MenuBuilders.InvokeSafe(builtMenus);

                    for (var x = 0; x < builtSettings.Count; x++)
                    {
                        var builtSetting = builtSettings[x];

                        if (builtSetting != null)
                        {
                            if (player.settingsAssignedIdLookup.ContainsKey(builtSetting.AssignedId))
                            {
                                ApiLog.Warn("Settings API",
                                    $"Skipping settings entry &1{builtSetting.CustomId}&r due to a duplicate ID ({builtSetting.AssignedId}).");
                                continue;
                            }

                            if (player.settingsIdLookup.ContainsKey(builtSetting.CustomId))
                            {
                                ApiLog.Warn("Settings API",
                                    $"Skipping settings entry &1{builtSetting.CustomId}&r due to a duplicate ID");
                                continue;
                            }

                            builtSetting.Player = player;

                            player.settingsIdLookup.Add(builtSetting.CustomId, builtSetting);
                            player.settingsAssignedIdLookup.Add(builtSetting.AssignedId, builtSetting);

                            ExPlayerEvents.OnSettingsEntryCreated(new(builtSetting, null, player));
                        }
                    }

                    for (var x = 0; x < builtMenus.Count; x++)
                    {
                        var builtMenu = builtMenus[x];

                        if (builtMenu != null)
                        {
                            if (player.settingsMenuLookup.ContainsKey(builtMenu.CustomId))
                            {
                                ApiLog.Warn("Settings API",
                                    $"Skipping settings menu &1{builtMenu.CustomId}&r due to a duplicate ID.");
                                continue;
                            }

                            var menuList = ListPool<SettingsEntry>.Shared.Rent();

                            builtMenu.Player = player;
                            builtMenu.BuildMenu(menuList);

                            builtMenu.Entries = ListPool<SettingsEntry>.Shared.ToArrayReturn(menuList);

                            player.settingsMenuLookup.Add(builtMenu.CustomId, builtMenu);

                            for (int y = 0; y < builtMenu.Entries.Length; y++)
                            {
                                var menuSetting = builtMenu.Entries[y];

                                if (menuSetting != null)
                                {
                                    menuSetting.Player = player;
                                    menuSetting.Menu = builtMenu;

                                    if (player.settingsIdLookup.ContainsKey(menuSetting.CustomId))
                                    {
                                        ApiLog.Warn("Settings API",
                                            $"Skipping menu settings entry &1{menuSetting.CustomId}&r due to a duplicate ID.");
                                        continue;
                                    }

                                    if (player.settingsAssignedIdLookup.ContainsKey(menuSetting.AssignedId))
                                    {
                                        ApiLog.Warn("Settings API",
                                            $"Skipping menu settings entry &1{menuSetting.CustomId}&r due to a duplicate ID.");
                                        continue;
                                    }

                                    player.settingsIdLookup.Add(menuSetting.CustomId, menuSetting);
                                    player.settingsAssignedIdLookup.Add(menuSetting.AssignedId, menuSetting);

                                    ExPlayerEvents.OnSettingsEntryCreated(new(menuSetting, builtMenu, player));
                                }
                            }
                        }
                    }

                    ListPool<SettingsEntry>.Shared.Return(builtSettings);
                    ListPool<SettingsMenu>.Shared.Return(builtMenus);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Settings API",
                        $"Failed while building settings for player &1{player.Nickname} ({player.UserId})&r at index &3{i}&r:\n{ex.ToColoredString()}");
                }
            }

            if (player.settingsIdLookup.Count > 0)
                SyncEntries(player);
        }
        catch (Exception ex)
        {
            ApiLog.Error("Settings API",
                $"Failed while building settings for player &1{player.Nickname}&r ({player.UserId})&r:\n{ex.ToColoredString()}");
        }
    }

    internal static void OnStatusMessage(NetworkConnection connection, SSSUserStatusReport userStatusReport)
    {
        try
        {
            if (connection is null || !ExPlayer.TryGet(connection, out var player) || !player)
                return;

            if (player.SettingsReport.HasValue)
            {
                ExPlayerEvents.OnSettingsStatusReportReceived(new(player, userStatusReport,
                    player.SettingsReport.Value));

                if (userStatusReport.TabOpen && !player.SettingsReport.Value.TabOpen)
                    ExPlayerEvents.OnSettingsTabOpened(new(player));
                else if (!userStatusReport.TabOpen && player.SettingsReport.Value.TabOpen)
                    ExPlayerEvents.OnSettingsTabClosed(new(player));

                player.SettingsReport = userStatusReport;
                return;
            }

            ExPlayerEvents.OnSettingsStatusReportReceived(new(player, userStatusReport, null));

            if (userStatusReport.TabOpen)
                ExPlayerEvents.OnSettingsTabOpened(new(player));

            player.SettingsReport = userStatusReport;
        }
        catch (Exception ex)
        {
            ApiLog.Error("Settings API", $"Failed to handle status message!\n{ex.ToColoredString()}");
        }
    }

    internal static void OnResponseMessage(NetworkConnection connection, SSSClientResponse clientResponse)
    {
        try
        {
            if (connection is null || !ExPlayer.TryGet(connection, out var player) || player is null)
                return;

            if (clientResponse.SettingType is null)
                return;

            if (!player.TryGetEntry(clientResponse.Id, out var entry))
                return;

            if (entry.Base is null)
                return;

            if (entry.Base.GetType() != clientResponse.SettingType)
                return;

            if (entry.Base.ResponseMode is ServerSpecificSettingBase.UserResponseMode.None)
                return;

            using var reader = NetworkReaderPool.Get(clientResponse.Payload);

            if (entry is ICustomReaderSetting customReaderSetting)
                customReaderSetting.Read(reader);
            else
                entry.Base.DeserializeValue(reader);

            entry.InternalOnUpdated();

            ExPlayerEvents.OnSettingsEntryUpdated(new(entry));

            if (entry.Menu != null)
            {
                switch (entry)
                {
                    case SettingsButton button:
                        entry.Menu.OnButtonTriggered(button);
                        break;

                    case SettingsTwoButtons twoButtons:
                        entry.Menu.OnButtonSwitched(twoButtons);
                        break;

                    case SettingsPlainText plainText:
                        entry.Menu.OnPlainTextUpdated(plainText);
                        break;

                    case SettingsSlider slider:
                        entry.Menu.OnSliderMoved(slider);
                        break;

                    case SettingsTextArea textArea:
                        entry.Menu.OnTextInput(textArea);
                        break;

                    case SettingsKeyBind keyBind:
                        entry.Menu.OnKeyBindPressed(keyBind);
                        break;

                    case SettingsDropdown dropdown:
                        entry.Menu.OnDropdownSelected(dropdown, dropdown.SelectedOption);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Settings API", $"Failed to handle response message!\n{ex.ToColoredString()}");
        }
    }

    internal static void Internal_Init()
        => InternalEvents.OnPlayerVerified += OnPlayerVerified;
}