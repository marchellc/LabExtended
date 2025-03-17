using LabExtended.Attributes;

using Mirror;

using UserSettings.ServerSpecific;

using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Interfaces;

using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.API.Settings
{
    public static class SettingsManager
    {
        private static readonly List<SettingsBuilder> allBuilders = new List<SettingsBuilder>();
        private static readonly List<SettingsEntry> allEntries = new List<SettingsEntry>();
        private static readonly List<SettingsMenu> allMenus = new List<SettingsMenu>();

        public static IReadOnlyList<SettingsBuilder> AllBuilders => allBuilders;
        public static IReadOnlyList<SettingsEntry> AllEntries => allEntries;
        public static IReadOnlyList<SettingsMenu> AllMenus => allMenus;
        
        public static int Version
        {
            get => ServerSpecificSettingsSync.Version;
            set => ServerSpecificSettingsSync.Version = value;
        }

        public static void SyncEntries(ExPlayer player)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));
            
            ApiLog.Debug("Settings API", $"Synchronizing entries for player &3{player.Nickname}&r (&6{player.UserId}&r)");
            
            var list = ListPool<ServerSpecificSettingBase>.Shared.Rent();
            var headers = ListPool<string>.Shared.Rent();

            foreach (var settingsEntry in player.settingsIdLookup)
            {
                ApiLog.Debug("Settings API", $"Processing entry &1{settingsEntry.Key}&r");

                if (settingsEntry.Value is null || settingsEntry.Value.Base is null)
                {
                    ApiLog.Debug("Settings API", $"Value or it's base is null (Base={settingsEntry.Value.Base is null})");
                    continue;
                }

                if (!settingsEntry.Value.Player)
                {
                    ApiLog.Debug("Settings API", $"Entry player is null");
                    continue;
                }

                if (settingsEntry.Value.IsHidden)
                {
                    ApiLog.Debug("Settings API", $"Entry is hidden");
                    continue;
                }

                if (settingsEntry.Value.Menu != null)
                {
                    ApiLog.Debug("Settings API", $"Entry belongs to menu");

                    if (settingsEntry.Value.Menu.IsHidden)
                    {
                        ApiLog.Debug("Settings API", $"Entry parent menu is hidden");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(settingsEntry.Value.Menu.Header) &&
                        !headers.Contains(settingsEntry.Value.Menu.CustomId))
                    {
                        headers.Add(settingsEntry.Value.Menu.CustomId);
                        list.Add(new SSGroupHeader(settingsEntry.Value.Menu.Header, settingsEntry.Value.Menu.HeaderReducedPadding, settingsEntry.Value.Menu.HeaderHint));
                        
                        ApiLog.Debug("Settings API", $"Added group header &1{settingsEntry.Value.Menu.Header}&r (&6{settingsEntry.Value.Menu.CustomId}&r)");
                    }
                }
                
                list.Add(settingsEntry.Value.Base);
                
                ApiLog.Debug("Settings API", $"Added entry &1{settingsEntry.Value.CustomId}&r (&6{settingsEntry.Value.AssignedId}&r)");
            }
            
            ApiLog.Debug("Settings API", $"Sending &1{list.Count}&r entries");

            player.Send(new SSSEntriesPack(ListPool<ServerSpecificSettingBase>.Shared.ToArrayReturn(list), Version));
            
            ListPool<string>.Shared.Return(headers);
        }

        public static bool TryGetMenu<TMenu>(ExPlayer player, out TMenu menu) where TMenu : SettingsMenu
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
        
        public static bool TryGetMenu(string menuId, ExPlayer player, out SettingsMenu menu)
            => player.settingsMenuLookup.TryGetValue(menuId, out menu);
        
        public static bool TryGetEntry(int generatedId, ExPlayer? player, out SettingsEntry entry)
            => player.settingsAssignedIdLookup.TryGetValue(generatedId, out entry);
        
        public static bool TryGetEntry(string customId, ExPlayer player, out SettingsEntry entry)
            => player.settingsIdLookup.TryGetValue(customId, out entry);
        
        public static bool TryGetEntry(Func<SettingsEntry, bool> predicate, out SettingsEntry entry)
            => (entry = GetEntry(predicate)) != null;

        public static TMenu GetMenu<TMenu>(ExPlayer player) where TMenu : SettingsMenu
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            foreach (var menu in player.settingsMenuLookup)
            {
                if (menu.Value is TMenu value)
                    return value;
            }

            throw new Exception($"Could not find a menu of type {typeof(TMenu).Name}");
        }

        public static SettingsMenu GetMenu(string menuId, ExPlayer player)
            => player.settingsMenuLookup[menuId];

        public static SettingsEntry GetEntry(string customId, ExPlayer player)
            => player.settingsIdLookup[customId];
        
        public static SettingsEntry GetEntry(int generatedId, ExPlayer player)
            => player.settingsAssignedIdLookup[generatedId];
        
        public static SettingsEntry GetEntry(Func<SettingsEntry, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            return allEntries.TryGetFirst(x => predicate(x), out var entry) ? entry : null;
        }
        
        public static IEnumerable<SettingsEntry> GetEntries(int generatedId)
            => GetEntries(x => x.AssignedId == generatedId);

        public static IEnumerable<SettingsEntry> GetEntries(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))   
                throw new ArgumentNullException(nameof(customId));
            
            return GetEntries(x => x.CustomId == customId);
        }
        
        public static IEnumerable<SettingsEntry> GetEntries(ExPlayer player)
        {
            if (player is null || !player)
                throw new ArgumentNullException(nameof(player));

            return player.settingsIdLookup.Values;
        }

        public static IEnumerable<SettingsEntry> GetEntries(Func<SettingsEntry, bool> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate)); 
            
            return allEntries.Where(predicate);
        }

        public static void AddBuilder(SettingsBuilder settingsBuilder)
        {
            if (settingsBuilder is null)
                throw new ArgumentNullException(nameof(settingsBuilder));

            allBuilders.Add(settingsBuilder);
        }

        public static void RemoveBuilder(string builderId)
        {
            if (string.IsNullOrWhiteSpace(builderId))
                throw new ArgumentNullException(nameof(builderId));

            allBuilders.RemoveAll(x => !string.IsNullOrWhiteSpace(x.CustomId) && x.CustomId == builderId);
        }

        public static bool HasBuilder(string builderId)
        {
            if (string.IsNullOrWhiteSpace(builderId))
                return false;

            return allBuilders.Any(x => !string.IsNullOrWhiteSpace(x.CustomId) && x.CustomId == builderId);
        }

        public static void AddMenu(ExPlayer player, SettingsMenu menu)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (menu is null)
                throw new ArgumentNullException(nameof(menu));

            if (player.settingsMenuLookup.ContainsKey(menu.CustomId))
                throw new Exception($"A menu with the same ID already exists for {player.Nickname} ({player.UserId})");

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

            for (int y = 0; y < menu.Entries.Length; y++)
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
        }

        public static void AddSetting(ExPlayer player, SettingsEntry entry)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            if (player.settingsIdLookup.ContainsKey(entry.CustomId))
                throw new Exception($"An entry with the same custom ID ({entry.CustomId}) already exists for player {player.Nickname} ({player.UserId})");

            if (player.settingsAssignedIdLookup.ContainsKey(entry.AssignedId))
                throw new Exception($"An entry with the same assigned ID ({entry.AssignedId}) already exists for player {player.Nickname} ({player.UserId})");

            entry.Player = player;
            
            player.settingsIdLookup.Add(entry.CustomId, entry);
            player.settingsAssignedIdLookup.Add(entry.AssignedId, entry);

            SyncEntries(player);
        }

        public static void AddSettings(ExPlayer player, IEnumerable<SettingsEntry> entries, string groupHeader = null, bool reducedHeaderPadding = false, string headerHint = null)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));   
            
            if (entries is null)
                throw new ArgumentNullException(nameof(entries));

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
        }

        public static bool HasSettingsOpen(this ExPlayer player)
        {
            if (!player || !player.SettingsReport.HasValue)
                return false;

            return player.SettingsReport.Value.TabOpen;
        }

        public static int GetUserVersion(this ExPlayer player)
        {
            if (!player || !player.SettingsReport.HasValue)
                return 0;

            return player.SettingsReport.Value.Version;
        }

        public static int GetIntegerId(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            return customId.GetStableHashCode();
        }

        private static void OnPlayerLeft(ExPlayer player)
        {
            foreach (var menu in player.settingsMenuLookup.Values)
            {
                if (menu is IDisposable disposable)
                    disposable.Dispose();

                allMenus.Remove(menu);
            }
            
            foreach (var entry in player.settingsIdLookup.Values)
            {
                if (entry is IDisposable disposable)
                    disposable.Dispose();

                allEntries.Remove(entry);
            }
        }

        private static void OnPlayerJoined(ExPlayer player)
        {
            try
            {
                if (!player)
                    return;
                
                for (int i = 0; i < allBuilders.Count; i++)
                {
                    try
                    {
                        var builder = allBuilders[i];

                        if (builder.Predicate != null && !builder.Predicate(player))
                            continue;

                        var builtSettings = ListPool<SettingsEntry>.Shared.Rent();
                        var builtMenus = ListPool<SettingsMenu>.Shared.Rent();

                        builder.SettingsBuilders.InvokeSafe(builtSettings);
                        builder.MenuBuilders.InvokeSafe(builtMenus);
                        
                        for (int x = 0; x < builtSettings.Count; x++)
                        {
                            var builtSetting = builtSettings[x];

                            if (builtSetting != null)
                            {
                                if (player.settingsAssignedIdLookup.ContainsKey(builtSetting.AssignedId))
                                {
                                    ApiLog.Warn("Settings API", $"Skipping settings entry &1{builtSetting.CustomId}&r due to a duplicate ID ({builtSetting.AssignedId}).");
                                    continue;
                                }

                                if (player.settingsIdLookup.ContainsKey(builtSetting.CustomId))
                                {
                                    ApiLog.Warn("Settings API", $"Skipping settings entry &1{builtSetting.CustomId}&r due to a duplicate ID");
                                    continue;
                                }
                                
                                builtSetting.Player = player;
                                
                                player.settingsIdLookup.Add(builtSetting.CustomId, builtSetting);
                                player.settingsAssignedIdLookup.Add(builtSetting.AssignedId, builtSetting);

                                ExPlayerEvents.OnSettingsEntryCreated(new(builtSetting, null, player));
                            }
                        }

                        for (int x = 0; x < builtMenus.Count; x++)
                        {
                            var builtMenu = builtMenus[x];

                            if (builtMenu != null)
                            {
                                if (player.settingsMenuLookup.ContainsKey(builtMenu.CustomId))
                                {
                                    ApiLog.Warn("Settings API", $"Skipping settings menu &1{builtMenu.CustomId}&r due to a duplicate ID.");
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
                                            ApiLog.Warn("Settings API", $"Skipping menu settings entry &1{menuSetting.CustomId}&r due to a duplicate ID.");
                                            continue;
                                        }

                                        if (player.settingsAssignedIdLookup.ContainsKey(menuSetting.AssignedId))
                                        {
                                            ApiLog.Warn("Settings API", $"Skipping menu settings entry &1{menuSetting.CustomId}&r due to a duplicate ID.");
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
                        ApiLog.Error("Settings API", $"Failed while building settings for player &1{player.Nickname} ({player.UserId})&r at index &3{i}&r:\n{ex.ToColoredString()}");
                    }
                }

                if (player.settingsIdLookup.Count > 0)
                {
                    SyncEntries(player);
                    
                    ApiLog.Debug("Settings API", $"Built &1{player.settingsIdLookup.Count}&r setting entries " +
                                                 $"and &1{player.settingsMenuLookup.Count}&r menu(s) for player " +
                                                 $"&1{player.Nickname}&r (&6{player.UserId}&r)");
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Settings API", $"Failed while building settings for player &1{player.Nickname}&r ({player.UserId})&r:\n{ex.ToColoredString()}");
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
                ApiLog.Debug("Settings API", $"Received response, Id={clientResponse.Id}, Type={clientResponse.SettingType?.Name ?? "null"}, Payload={clientResponse.Payload?.Length ?? -1}");
                
                if (connection is null || !ExPlayer.TryGet(connection, out var player) || !player)
                {
                    ApiLog.Debug("Settings API", $"Failed to retrieve player from connection &1{connection?.connectionId ?? -1}&r");
                    return;
                }

                if (clientResponse.SettingType is null)
                {
                    ApiLog.Debug("Settings API", $"Player &1{player.Nickname} ({player.UserId})&r sent an empty setting type");
                    return;
                }

                if (!TryGetEntry(clientResponse.Id, player, out var entry))
                {
                    ApiLog.Debug("Settings API", $"Failed to find setting ID &1{clientResponse.Id}&r for player &1{player.Nickname} ({player.UserId})&r!");
                    return;
                }

                if (entry.Base is null)
                {
                    ApiLog.Debug("Settings API", $"Base of entry &1{clientResponse.Id}&r is null");
                    return;
                }

                if (entry.Base.GetType() != clientResponse.SettingType)
                {
                    ApiLog.Debug("Settings API", $"Entry setting type mismatch (expected &1{entry.Base.GetType().Name}&r, received &1{clientResponse.SettingType.Name}&r)");
                    return;
                }

                if (entry.Base.ResponseMode is ServerSpecificSettingBase.UserResponseMode.None)
                {
                    ApiLog.Debug("Settings API", $"Received update for a non-responsive settings entry");
                    return;
                }

                ApiLog.Debug("Settings API", $"Updating entry &1{entry.CustomId}&r ({entry.AssignedId}) (menu: {entry.Menu?.CustomId ?? "null"})");

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

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnPlayerLeft += OnPlayerLeft;
            InternalEvents.OnPlayerJoined += OnPlayerJoined;
        }
    }
}