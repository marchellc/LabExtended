using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Events;
using System.Diagnostics;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings {
    public static class VanillaSettingsAdapter {

        public static Dictionary<Assembly, ServerSpecificSettingBase[]> SssByAssemblyGlobal { get; } = new();

        public static Dictionary<ExPlayer, Dictionary<Assembly, ServerSpecificSettingBase[]>> SssByAssemblyPersonal { get; } = new();

        public static void SyncPersonalSettings(ExPlayer? player, Assembly assembly, ServerSpecificSettingBase[] collection) {
            if (player == null) {
                ApiLog.Warn($"Player is null in SyncPersonalSettings()");
                return;
            }

            Dictionary<Assembly, ServerSpecificSettingBase[]> playerSettings;
            if (!SssByAssemblyPersonal.TryGetValue(player, out playerSettings)) {
                playerSettings = SssByAssemblyPersonal[player] = new();
            }

            if (collection == null || collection.Length == 0) {
                if (playerSettings.Remove(assembly) && playerSettings.Count == 0) {
                    SssByAssemblyPersonal.Remove(player);
                }
            } else {
                playerSettings[assembly] = collection;
            }
        }


        internal static Assembly GetCallerAssembly() {
            Assembly assembly = null;
            StackFrame[] frames = new StackTrace().GetFrames();
            for (int i = 2; i < frames.Length; i++) {
                StackFrame frame = frames[i];
                assembly = frame.GetMethod().DeclaringType.Assembly;
                if (!IsIgnoredAssembly(assembly)) {
                    return assembly;
                }
            }

            if (IsIgnoredAssembly(assembly)) {
                return AssemblyCSharp;
            }

            throw new Exception("Couldn't get calling Assembly of SettingsDefinitions property");
        }

        private static Assembly AssemblyCSharp = typeof(ReferenceHub).Assembly;
        private static Assembly HarmonyAssembly = typeof(HarmonyLib.Harmony).Assembly;
        private static Assembly MirrorAssembly = typeof(Mirror.NetworkIdentity).Assembly;
        private static Assembly LabApiAssembly = typeof(LabApi.Loader.PluginLoader).Assembly;

        private static bool IsIgnoredAssembly(Assembly? assembly) {
            return assembly == null ||
                assembly.Equals(AssemblyCSharp) ||
                assembly.Equals(HarmonyAssembly) ||
                assembly.Equals(MirrorAssembly) ||
                assembly.Equals(LabApiAssembly);
        }

        private static void OnPlayerLeft(ExPlayer player) {
            SssByAssemblyPersonal.Remove(player);
        }

        [LoaderInitialize(1)]
        private static void OnInit() {
            InternalEvents.OnPlayerLeft += OnPlayerLeft;
        }
    }
}
