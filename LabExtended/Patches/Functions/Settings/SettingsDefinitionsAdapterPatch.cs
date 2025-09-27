using HarmonyLib;
using LabExtended.API;
using LabExtended.API.Settings;
using LabExtended.Core;
using System.Reflection;
using UserSettings.ServerSpecific;

using static LabExtended.API.Settings.VanillaSettingsAdapter;

namespace LabExtended.Patches.Functions.Settings {
    class SettingsDefinitionsAdapterPatch {

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
        private static bool DefinedSettingsGetterPrefix(ref ServerSpecificSettingBase[] __result) {
            Assembly assembly =  GetCallerAssembly();

            if (!SssByAssemblyGlobal.TryGetValue(assembly, out __result) || __result == null) {
                __result = [];
            }

            ApiLog.Debug($"[{assembly.GetName().Name}] DefinedSettings.Getter, get length: " + __result.Length);
            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
        private static bool DefinedSettingsSetterPrefix(ref ServerSpecificSettingBase[] value) {
            Assembly assembly = GetCallerAssembly();

            if (value == null || value.Length == 0) {
                SssByAssemblyGlobal.Remove(assembly);
            } else {
                SssByAssemblyGlobal[assembly] = value;
            }

            ApiLog.Debug($"[{assembly.GetName().Name}] DefinedSettings.Setter, new length: " + (value?.Length ?? 0));
            return false;
        }


        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
        private static bool SendToAllPrefix() {
            Assembly assembly = GetCallerAssembly();
            foreach (var player in ExPlayer.Players) {
                SyncPersonalSettings(player, assembly, []);
                player.SyncEntries();
            }

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
        private static bool SendToSpecificHubPrefix(ReferenceHub hub) {
            Assembly assembly = GetCallerAssembly();
            if (!ExPlayer.TryGet(hub, out var player)) {
                return false;
            }

            SyncPersonalSettings(player, assembly, []);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
        private static bool SendToSpecificHubWithSettingsPrefix(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null) {
            Assembly assembly = GetCallerAssembly();
            if (ExPlayer.TryGet(hub, out var player)) {
                return false;
            }

            SyncPersonalSettings(player, assembly, collection);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        private static bool SendToConditionallyPrefix(Func<ReferenceHub, bool> filter) {
            Assembly assembly = GetCallerAssembly();
            if (SssByAssemblyGlobal.TryGetValue(assembly, out ServerSpecificSettingBase[] settings)) {
                foreach (var player in ExPlayer.Players) {
                    if (filter(player.ReferenceHub)) {
                        SyncPersonalSettings(player, assembly, []);
                        player.SyncEntries();
                    }
                }
            }

            return false;
        }
    }
}
