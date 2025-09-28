using HarmonyLib;
using LabExtended.API;
using LabExtended.API.Settings;
using LabExtended.Core;
using System.Diagnostics;
using System.Reflection;
using UserSettings.ServerSpecific;
using static LabExtended.API.Settings.SettingsManager;

namespace LabExtended.Patches.Functions.Settings
{
    class VanillaSettingsAdapterPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
        private static bool DefinedSettingsGetterPrefix(ref ServerSpecificSettingBase[] __result)
        {
            Assembly assembly = GetCallerAssembly();

            if (!GlobalSettingsByAssembly.TryGetValue(assembly, out __result) || __result == null)
                __result = [];

            ApiLog.Debug($"[{assembly.GetName().Name}] DefinedSettings.Getter, get length: " + __result.Length);
            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
        private static bool DefinedSettingsSetterPrefix(ref ServerSpecificSettingBase[] value) {
            Assembly assembly = GetCallerAssembly();

            if (value == null || value.Length == 0)
                GlobalSettingsByAssembly.Remove(assembly);
            else
                GlobalSettingsByAssembly[assembly] = value;

            ApiLog.Debug($"[{assembly.GetName().Name}] DefinedSettings.Setter, new length: " + (value?.Length ?? 0));
            return false;
        }


        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Getter)]
        private static bool SendOnJoinFilterGetterPrefix(ref Predicate<ReferenceHub> __result) {
            // todo getter
            return true;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendOnJoinFilter), MethodType.Setter)]
        private static bool SendOnJoinFilterSetterPrefix(ref Predicate<ReferenceHub> value) {
            // todo setter + Implement SendOnJoinFilter check for each assembly (after some sleep)
            return true;
        }


        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToAll))]
        private static bool SendToAllPrefix()
        {
            Assembly assembly = GetCallerAssembly();
            foreach (var player in ExPlayer.Players)
            {
                player.SyncSettingsByAssembly(assembly, []);
                player.SyncEntries();
            }

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
        private static bool SendToSpecificHubPrefix(ReferenceHub hub)
        {
            Assembly assembly = GetCallerAssembly();
            if (!ExPlayer.TryGet(hub, out var player))
                return false;

            player.SyncSettingsByAssembly(assembly, []);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
        private static bool SendToSpecificHubWithSettingsPrefix(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            Assembly assembly = GetCallerAssembly();
            if (ExPlayer.TryGet(hub, out var player))
                return false;

            player.SyncSettingsByAssembly(assembly, collection);
            player.SyncEntries();

            return false;
        }

        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayersConditionally))]
        private static bool SendToConditionallyPrefix(Func<ReferenceHub, bool> filter)
        {
            Assembly assembly = GetCallerAssembly();
            if (GlobalSettingsByAssembly.TryGetValue(assembly, out ServerSpecificSettingBase[] settings))
            {
                foreach (var player in ExPlayer.Players)
                {
                    if (filter(player.ReferenceHub))
                    {
                        player.SyncSettingsByAssembly(assembly, []);
                        player.SyncEntries();
                    }
                }
            }

            return false;
        }


        internal static Assembly GetCallerAssembly() {
            Assembly assembly = null;
            StackFrame[] frames = new StackTrace().GetFrames();
            for (int i = 2; i < frames.Length; i++) {
                MethodBase method = frames[i].GetMethod();
                assembly = method.DeclaringType?.Assembly ?? method.ReflectedType.Assembly;
                if (!IsIgnoredAssembly(assembly))
                    return assembly;
            }

            if (IsIgnoredAssembly(assembly))
                return AssemblyCSharp;

            throw new Exception("Couldn't get calling Assembly of SettingsDefinitions property");
        }

        private static Assembly AssemblyCSharp = typeof(ReferenceHub).Assembly;
        private static Assembly HarmonyAssembly = typeof(HarmonyLib.Harmony).Assembly;
        private static Assembly MirrorAssembly = typeof(Mirror.NetworkIdentity).Assembly;
        private static Assembly LabApiAssembly = typeof(LabApi.Loader.PluginLoader).Assembly;

        private static bool IsIgnoredAssembly(Assembly? assembly) =>
            assembly == null ||
            assembly.Equals(AssemblyCSharp) ||
            assembly.Equals(HarmonyAssembly) ||
            assembly.Equals(MirrorAssembly) ||
            assembly.Equals(LabApiAssembly);
    }
}
