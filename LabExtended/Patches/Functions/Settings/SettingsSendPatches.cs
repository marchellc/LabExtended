using HarmonyLib;

using LabExtended.API;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    /// <summary>
    /// Implements server-specific settings from the custom API (prevents overriding by base-game settings API).
    /// </summary>
    public static class SettingsSendPatches
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
        private static bool SendToAllPrefix()
        {
            var settings = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();

            for (var i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (player?.ReferenceHub == null || player.settingsList is null)
                    continue;

                var playerSettings = settings;

                if (player.settingsList.Count > 0)
                    playerSettings = player.settingsList.Concat(playerSettings).ToArray();

                if (playerSettings.Length > 0)
                    player.Send(new SSSEntriesPack(playerSettings, ServerSpecificSettingsSync.Version));
            }

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        private static bool SendToConditionallyPrefix(Func<ReferenceHub, bool> filter)
        {
            var settings = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();

            for (var i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (player?.ReferenceHub == null || player.settingsList is null)
                    continue;

                if (!filter(player.ReferenceHub))
                    continue;

                var playerSettings = settings;

                if (player.settingsList.Count > 0)
                    playerSettings = player.settingsList.Concat(playerSettings).ToArray();

                if (playerSettings.Length > 0)
                    player.Send(new SSSEntriesPack(playerSettings, ServerSpecificSettingsSync.Version));
            }

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
        private static bool SendToPlayerPrefix(ReferenceHub hub)
        {
            if (!ExPlayer.TryGet(hub, out var player))
                return true;

            if (player?.ReferenceHub == null || player.settingsIdLookup is null)
                return true;

            var playerSettings = ServerSpecificSettingsSync.DefinedSettings ?? Array.Empty<ServerSpecificSettingBase>();

            if (player.settingsList.Count > 0)
                playerSettings = player.settingsList.Concat(playerSettings).ToArray();

            if (playerSettings.Length > 0)
                player.Send(new SSSEntriesPack(playerSettings, ServerSpecificSettingsSync.Version));

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), 
            typeof(ReferenceHub), 
            typeof(ServerSpecificSettingBase[]), 
            typeof(int?))]
        private static bool SendToPlayerPrefix(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            if (!ExPlayer.TryGet(hub, out var player))
                return true;

            if (player?.ReferenceHub == null || player.settingsList is null)
                return true;

            var playerSettings = collection ?? Array.Empty<ServerSpecificSettingBase>();

            if (player.settingsList.Count > 0)
                playerSettings = player.settingsList.Concat(playerSettings).ToArray();

            if (playerSettings.Length > 0)
                player.Send(new SSSEntriesPack(playerSettings, versionOverride ?? ServerSpecificSettingsSync.Version));

            return false;
        }
    }
}