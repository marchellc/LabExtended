using LabExtended.Attributes;
using LabExtended.Patches.Functions.Players;

using Mirror;

using UserSettings.ServerSpecific;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;
using LabExtended.API.Settings.Entries.Dropdown;
using LabExtended.API.Settings.Menus;

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
        private static LockedList<SettingsBuilder> allBuilders = new LockedList<SettingsBuilder>();
        private static LockedDictionary<ExPlayer, SSSUserStatusReport> allStatuses = new LockedDictionary<ExPlayer, SSSUserStatusReport>();
        private static LockedDictionary<string, LockedDictionary<ExPlayer, SettingsEntry>> idEntries = new LockedDictionary<string, LockedDictionary<ExPlayer, SettingsEntry>>();

        public static IReadOnlyList<SettingsBuilder> AllBuilders => allBuilders;
        public static IReadOnlyDictionary<ExPlayer, SSSUserStatusReport> AllStatuses => allStatuses;

        public static IEnumerable<SettingsEntry> AllEntries => idEntries.SelectMany(x => x.Value.Values);
        
        public static int Version
        {
            get => ServerSpecificSettingsSync.Version;
            set => ServerSpecificSettingsSync.Version = value;
        }

        public static event Action<ExPlayer, SettingsEntry> OnUpdated;
        public static event Action<ExPlayer, SettingsEntry> OnCreated; 
        
        public static void SyncEntries(ExPlayer player)
            => player?.Connection?.Send(new SSSEntriesPack(GetEntries(player).Select(x => x.Base).ToArray(), Version));

        // for some odd reason this just breaks if I remove debug lines??
        public static bool TryGetEntry(int generatedId, ExPlayer player, out SettingsEntry entry)
            => TryGetEntry(x =>
            {
                if (x.AssignedId != generatedId)
                {
                    ApiLog.Debug("Settings API", $"ID comparison failed ({x.AssignedId} / {generatedId})");
                    return false;
                }

                if (x.Player != player)
                {
                    ApiLog.Debug("Settings API", $"Player comparison failed ({x.Player.UserId} - {x.Player.GetHashCode()} / {player.UserId} - {player.GetHashCode()})");
                    return false;
                }

                return true;
            }, out entry);
        
        public static bool TryGetEntry(string customId, ExPlayer player, out SettingsEntry entry)
            => TryGetEntry(x => x.CustomId == customId && x.Player == player, out entry);
        
        public static bool TryGetEntry(Predicate<SettingsEntry> predicate, out SettingsEntry entry)
            => (entry = GetEntry(predicate)) != null;
        
        public static SettingsEntry GetEntry(string customId, ExPlayer player)
            => GetEntry(x => x.CustomId == customId && x.Player == player);
        
        public static SettingsEntry GetEntry(int generatedId, ExPlayer player)
            => GetEntry(x => x.AssignedId == generatedId && x.Player == player);
        
        public static SettingsEntry GetEntry(Predicate<SettingsEntry> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));
            
            foreach (var idPair in idEntries)
            {
                foreach (var entryPair in idPair.Value)
                {
                    if (!predicate(entryPair.Value))
                        continue;
                    
                    return entryPair.Value;
                }
            }
            
            return null;
        }
        
        public static List<SettingsEntry> GetEntries(int generatedId)
            => GetEntries(x => x.AssignedId == generatedId);

        public static List<SettingsEntry> GetEntries(string customId)
        {
            if (string.IsNullOrWhiteSpace(customId))   
                throw new ArgumentNullException(nameof(customId));
            
            return GetEntries(x => x.CustomId == customId);
        }
        
        public static List<SettingsEntry> GetEntries(ExPlayer player)
        {
            if (player is null || !player)
                throw new ArgumentNullException(nameof(player));
            
            return GetEntries(x => x.Player == player);
        }

        public static List<SettingsEntry> GetEntries(Predicate<SettingsEntry> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate)); 
            
            var list = new List<SettingsEntry>();

            foreach (var idPair in idEntries)
            {
                foreach (var entryPair in idPair.Value)
                {
                    if (!predicate(entryPair.Value))
                        continue;
                    
                    list.Add(entryPair.Value);
                }
            }
            
            return list;
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

        public static void AddSetting(ExPlayer player, SettingsEntry entry)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            entry.Player = player;

            if (!idEntries.TryGetValue(entry.CustomId, out var entries))
                idEntries.Add(entry.CustomId, entries = new LockedDictionary<ExPlayer, SettingsEntry>());

            entries[player] = entry;

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
                if (!idEntries.TryGetValue(groupHeader, out var savedIds))
                    idEntries[groupHeader] = savedIds = new LockedDictionary<ExPlayer, SettingsEntry>();

                savedIds[player] = new SettingsGroup(groupHeader, reducedHeaderPadding, headerHint);
            }
            
            foreach (var entry in entries)
            {
                entry.Player = player;

                if (!idEntries.TryGetValue(entry.CustomId, out var savedEntries))
                    idEntries.Add(entry.CustomId, savedEntries = new LockedDictionary<ExPlayer, SettingsEntry>());

                savedEntries[player] = entry;
            }

            SyncEntries(player);
        }

        public static bool HasSettingsOpen(ExPlayer player)
        {
            if (!player)
                return false;

            if (!allStatuses.TryGetValue(player, out var status))
                return false;

            return status.TabOpen;
        }

        public static int GetUserVersion(ExPlayer player)
        {
            if (!player || !allStatuses.TryGetValue(player, out var status))
                return 0;

            return status.Version;
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

                                if (!idEntries.TryGetValue(builtSetting.CustomId, out var idSettings))
                                    idEntries.Add(builtSetting.CustomId, idSettings = new LockedDictionary<ExPlayer, SettingsEntry>());
                                
                                idSettings[player] = builtSetting;

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
                                
                                if (!string.IsNullOrWhiteSpace(builtMenu.MenuLabel))
                                    builtList.Add(new SSGroupHeader(builtMenu.MenuLabel));
                                
                                builtMenu.BuildMenu(menuList);

                                builtMenu.Settings.Clear();
                                builtMenu.Settings.AddRange(menuList);

                                ListPool<SettingsEntry>.Shared.Return(menuList);

                                for (int y = 0; y < builtMenu.Settings.Count; y++)
                                {
                                    var menuSetting = builtMenu.Settings[y];

                                    if (menuSetting != null)
                                    {
                                        menuSetting.Player = player;
                                        menuSetting.Menu = builtMenu;
                                        
                                        if (!idEntries.TryGetValue(menuSetting.CustomId, out var idSettings))
                                            idEntries.Add(menuSetting.CustomId, idSettings = new LockedDictionary<ExPlayer, SettingsEntry>());
                                        
                                        idSettings[player] = menuSetting;

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
                    player.Connection.Send(
                        new SSSEntriesPack(ListPool<ServerSpecificSettingBase>.Shared.ToArrayReturn(builtList),
                            Version));
                else
                    ListPool<ServerSpecificSettingBase>.Shared.Return(builtList);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Settings API", $"Failed while building settings for player &1{player.Name}&r ({player.UserId})&r:\n{ex.ToColoredString()}");
            }
        }

        private static void OnPlayerLeft(ExPlayer player)
        {
            foreach (var idPair in idEntries)
            {
                idPair.Value.Remove(player);
            }
        }

        internal static void OnStatusMessage(NetworkConnection connection, SSSUserStatusReport userStatusReport)
        {
            try
            {
                if (connection is null || !ExPlayer.TryGet(connection, out var player) || !player)
                    return;
                
                ApiLog.Debug("Settings API", $"Received status report from &3{player.Name} ({player.UserId})&r (&6Version={userStatusReport.Version}, TabOpen={userStatusReport.TabOpen}&r)");

                if (allStatuses.TryGetValue(player, out var curReport))
                {
                    HookRunner.RunEvent(new SettingsStatusReportReceivedArgs(player, userStatusReport, curReport));

                    allStatuses[player] = userStatusReport;
                    return;
                }
                
                HookRunner.RunEvent(new SettingsStatusReportReceivedArgs(player, userStatusReport, null));
                
                allStatuses.Add(player, userStatusReport);
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
                                entry.Menu.OnDropdownSelected(dropdown, dropdown.TryGetOption(dropdown.SelectedIndex, out var option) ? option : null);
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
            InternalEvents.OnPlayerLeft += OnPlayerLeft;
        }
    }
}