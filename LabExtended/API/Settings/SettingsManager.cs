using LabExtended.Attributes;

using Mirror;

using UserSettings.ServerSpecific;

using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Interfaces;

using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Events.Settings;

using NorthwoodLib.Pools;

namespace LabExtended.API.Settings
{
    public static class SettingsManager
    {
        private static readonly List<SettingsBuilder> allBuilders = new List<SettingsBuilder>();
        private static readonly List<SettingsEntry> allEntries = new List<SettingsEntry>();

        public static IReadOnlyList<SettingsBuilder> AllBuilders => allBuilders;
        public static IEnumerable<SettingsEntry> AllEntries => allEntries;
        
        public static int Version
        {
            get => ServerSpecificSettingsSync.Version;
            set => ServerSpecificSettingsSync.Version = value;
        }

        public static event Action<ExPlayer, SettingsEntry> OnUpdated;
        public static event Action<ExPlayer, SettingsEntry> OnCreated;

        public static void SyncEntries(ExPlayer player)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));
            
            var list = ListPool<ServerSpecificSettingBase>.Shared.Rent();

            foreach (var settingsEntry in player.settingsIdLookup)
            {
                if (settingsEntry.Value is null || settingsEntry.Value.Base is null)
                    continue;
                
                if (!settingsEntry.Value.Player)
                    continue;
                
                if (settingsEntry.Value.IsHidden)
                    continue;

                if (settingsEntry.Value.Menu != null && !settingsEntry.Value.Menu.IsActive)
                    continue;
                
                list.Add(settingsEntry.Value.Base);
            }

            player.Send(new SSSEntriesPack(ListPool<ServerSpecificSettingBase>.Shared.ToArrayReturn(list), Version));
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
        
        public static bool TryGetEntry(int generatedId, ExPlayer player, out SettingsEntry entry)
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
                throw new Exception($"A menu with the same ID already exists for {player.Name} ({player.UserId})");

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

                    HookRunner.RunEvent(new SettingsEntryCreatedArgs(menuSetting, menu, player));

                    OnCreated.InvokeSafe(player, menuSetting);
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
                throw new Exception($"An entry with the same custom ID ({entry.CustomId}) already exists for player {player.Name} ({player.UserId})");

            if (player.settingsAssignedIdLookup.ContainsKey(entry.AssignedId))
                throw new Exception($"An entry with the same assigned ID ({entry.AssignedId}) already exists for player {player.Name} ({player.UserId})");

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

        public static bool HasSettingsOpen(ExPlayer player)
        {
            if (!player || !player.sssReport.HasValue)
                return false;

            return player.sssReport.Value.TabOpen;
        }

        public static int GetUserVersion(ExPlayer player)
        {
            if (!player || !player.sssReport.HasValue)
                return 0;

            return player.sssReport.Value.Version;
        }

        public static int GetIntegerId(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            return customId.GetStableHashCode();
        }

        private static void OnPlayerJoined(ExPlayer player)
        {
            try
            {
                var builtList = ListPool<ServerSpecificSettingBase>.Shared.Rent();

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
                                builtSetting.Player = player;
                                
                                player.settingsIdLookup.Add(builtSetting.CustomId, builtSetting);
                                player.settingsAssignedIdLookup.Add(builtSetting.AssignedId, builtSetting);

                                HookRunner.RunEvent(new SettingsEntryCreatedArgs(builtSetting, null, player));

                                OnCreated.InvokeSafe(player, builtSetting);
                                
                                builtList.Add(builtSetting.Base);
                            }
                        }

                        for (int x = 0; x < builtMenus.Count; x++)
                        {
                            var builtMenu = builtMenus[x];

                            if (builtMenu != null)
                            {
                                var menuList = ListPool<SettingsEntry>.Shared.Rent();

                                builtMenu.Player = player;
                                builtMenu.BuildMenu(menuList);

                                builtMenu.Entries = ListPool<SettingsEntry>.Shared.ToArrayReturn(menuList);
                                
                                player.settingsMenuLookup.Add(builtMenu.CustomId, builtMenu);
                                
                                var menuHeader = new SettingsGroup(builtMenu.Header, builtMenu.HeaderReducedPadding, builtMenu.HeaderHint);

                                menuHeader.Menu = builtMenu;
                                menuHeader.Player = player;
                                
                                player.settingsIdLookup.Add(menuHeader.CustomId, menuHeader);
                                player.settingsAssignedIdLookup.Add(menuHeader.AssignedId, menuHeader);
                                
                                for (int y = 0; y < builtMenu.Entries.Length; y++)
                                {
                                    var menuSetting = builtMenu.Entries[y];

                                    if (menuSetting != null)
                                    {
                                        menuSetting.Player = player;
                                        menuSetting.Menu = builtMenu;
                                        
                                        player.settingsIdLookup.Add(menuSetting.CustomId, menuSetting);
                                        player.settingsAssignedIdLookup.Add(menuSetting.AssignedId, menuSetting);
                                        
                                        HookRunner.RunEvent(new SettingsEntryCreatedArgs(menuSetting, builtMenu, player));

                                        OnCreated.InvokeSafe(player, menuSetting);
                                        
                                        builtList.Add(menuSetting.Base);
                                    }
                                }
                            }
                        }

                        ListPool<SettingsEntry>.Shared.Return(builtSettings);
                        ListPool<SettingsMenu>.Shared.Return(builtMenus);
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("Settings API", $"Failed while building settings for player &1{player.Name} ({player.UserId})&r at index &3{i}&r:\n{ex.ToColoredString()}");
                    }
                }

                if (builtList.Count > 0)
                    SyncEntries(player);
                else
                    ListPool<ServerSpecificSettingBase>.Shared.Return(builtList);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Settings API", $"Failed while building settings for player &1{player.Name}&r ({player.UserId})&r:\n{ex.ToColoredString()}");
            }
        }

        internal static void OnStatusMessage(NetworkConnection connection, SSSUserStatusReport userStatusReport)
        {
            try
            {
                if (connection is null || !ExPlayer.TryGet(connection, out var player) || !player)
                    return;
                
                ApiLog.Debug("Settings API", $"Received status report from &3{player.Name} ({player.UserId})&r (&6Version={userStatusReport.Version}, TabOpen={userStatusReport.TabOpen}&r)");

                if (player.sssReport.HasValue)
                {
                    HookRunner.RunEvent(new SettingsStatusReportReceivedArgs(player, userStatusReport, player.sssReport.Value));

                    player.sssReport = userStatusReport;
                    return;
                }
                
                HookRunner.RunEvent(new SettingsStatusReportReceivedArgs(player, userStatusReport, null));

                player.sssReport = userStatusReport;
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
                    ApiLog.Debug("Settings API", $"Player &1{player.Name} ({player.UserId})&r sent an empty setting type");
                    return;
                }

                if (!TryGetEntry(clientResponse.Id, player, out var entry))
                {
                    ApiLog.Debug("Settings API", $"Failed to find setting ID &1{clientResponse.Id}&r for player &1{player.Name} ({player.UserId})&r!");
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

                using (var reader = NetworkReaderPool.Get(clientResponse.Payload))
                {
                    if (entry is ICustomReaderSetting customReaderSetting)
                        customReaderSetting.Read(reader);
                    else
                        entry.Base.DeserializeValue(reader);
                    
                    entry.InternalOnUpdated();

                    HookRunner.RunEvent(new SettingsEntryUpdatedArgs(entry));

                    OnUpdated.InvokeSafe(player, entry);

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
            }
            catch (Exception ex)
            {
                ApiLog.Error("Settings API", $"Failed to handle response message!\n{ex.ToColoredString()}");
            }
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            InternalEvents.OnPlayerJoined += OnPlayerJoined;
        }
    }
}