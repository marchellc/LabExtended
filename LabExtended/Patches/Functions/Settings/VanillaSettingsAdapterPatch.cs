using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Settings;

using LabExtended.Utilities;

using System.Reflection;

using UserSettings.ServerSpecific;

using static LabExtended.API.Settings.SettingsManager;

namespace LabExtended.Patches.Functions.Settings
{
    /// <summary>
    /// Provides Harmony patches to customize the behavior of server-specific settings synchronization for different
    /// assemblies.
    /// </summary>
    public static class VanillaSettingsAdapterPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
        private static bool DefinedSettingsGetterPrefix(ref ServerSpecificSettingBase[] __result)
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            if (!GlobalSettingsByAssembly.TryGetValue(assembly, out __result) || __result == null)
                __result = [];

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
        private static bool DefinedSettingsSetterPrefix(ref ServerSpecificSettingBase[] value) 
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            if (value == null || value.Length == 0)
                GlobalSettingsByAssembly.Remove(assembly);
            else
                GlobalSettingsByAssembly[assembly] = value;

            return false;
        }


        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Getter)]
        private static bool SendOnJoinFilterGetterPrefix(ref Predicate<ReferenceHub> __result) 
        {
            // todo getter
            return true;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Setter)]
        private static bool SendOnJoinFilterSetterPrefix(ref Predicate<ReferenceHub> value) 
        {
            // todo setter + Implement SendOnJoinFilter check for each assembly (after some sleep)
            return true;
        }


        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
        private static bool SendToAllPrefix()
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            for (var i = 0; i < ExPlayer.Players.Count; i++)
            {
                var player = ExPlayer.Players[i];

                player.SyncSettingsByAssembly(assembly, []);
                player.SyncEntries();
            }

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
        private static bool SendToSpecificHubPrefix(ReferenceHub hub)
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            if (!ExPlayer.TryGet(hub, out var player))
                return false;

            player.SyncSettingsByAssembly(assembly, []);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
        private static bool SendToSpecificHubWithSettingsPrefix(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            if (ExPlayer.TryGet(hub, out var player))
                return false;

            player.SyncSettingsByAssembly(assembly, collection);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        private static bool SendToConditionallyPrefix(Func<ReferenceHub, bool> filter)
        {
            var assembly = ReflectionUtils.GetCallerAssembly(2, true, IsIgnoredAssembly);

            if (IsIgnoredAssembly(assembly))
                assembly = ReflectionUtils.GameAssembly;

            if (GlobalSettingsByAssembly.TryGetValue(assembly, out var settings))
            {
                for (var i = 0; i < ExPlayer.Players.Count; i++)
                {
                    var player = ExPlayer.Players[i];

                    if (filter(player.ReferenceHub))
                    {
                        player.SyncSettingsByAssembly(assembly, []);
                        player.SyncEntries();
                    }
                }
            }

            return false;
        }

        private static bool IsIgnoredAssembly(Assembly? assembly) =>
            assembly == null ||
            assembly.Equals(ReflectionUtils.GameAssembly) ||
            assembly.Equals(ReflectionUtils.HarmonyAssembly) ||
            assembly.Equals(ReflectionUtils.MirrorAssembly) ||
            assembly.Equals(ReflectionUtils.LabApiAssembly);
    }
}
