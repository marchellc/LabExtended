using Common.Logging;

using HarmonyLib;

using LabExtended.API.Prefabs;

using LabExtended.Core.Hooking;
using LabExtended.Core.Logging;
using LabExtended.Core.Profiling;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Patches.Functions;
using LabExtended.Utilities;

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Loader;

namespace LabExtended.Core
{
    public class ExLoader
    {
        public static Version GameVersion { get; } = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);
        public static Version ApiVersion { get; } = new Version(1, 0, 0);

        public static ExLoader Loader { get; private set; }

        public static string Folder => Loader.Handler.PluginDirectoryPath;

        private readonly ProfilerMarker _marker = new ProfilerMarker("Startup");

        [PluginConfig]
        public ExLoaderConfig Config;
        public PluginHandler Handler;

        public Harmony Harmony;

        public VersionRange? GameCompatibility = new VersionRange(new Version(13, 5, 0));

        [PluginEntryPoint("LabExtended", "1.0.0", "An extension to NW's Plugin API.", "marchellc")]
        [PluginPriority(LoadPriority.Lowest)]
        public void Load()
        {
            _marker.MarkStart();

            try
            {
                Handler = PluginHandler.Get(this);
                Loader = this;

                Info("Extended Loader", $"Loading version &2{ApiVersion}&r ..");

                if (GameCompatibility.HasValue && !GameCompatibility.Value.InRange(GameVersion))
                {
                    Error("Extended Loader", $"Attempted to load for an unsupported game version (&1{GameVersion}&r) - supported: &2{GameCompatibility.Value}&r");
                    return;
                }

                if (Config is null)
                {
                    Error("Extended Loader", "The plugin's config is missing! Seems like you made an error. Delete it and restart the server.");
                    return;
                }

                if (Config.Logging.PipeEnabled && !Config.Logging.DisabledSources.Contains("common"))
                {
                    var logger = new ExLogger();

                    foreach (var output in LogOutput.Outputs)
                        output.AddLogger(logger);

                    LogOutput.DefaultLoggers.Add(logger);

                    if (Config.Logging.DebugEnabled && !Config.Logging.DisabledSources.Contains("common_debug"))
                    {
                        LogOutput.EnableForAll(LogLevel.Debug | LogLevel.Verbose | LogLevel.Trace);
                        LogUtils.Default = LogUtils.General | LogUtils.Debug;
                    }
                }

                Harmony = new Harmony($"com.extended.loader.{DateTime.Now.Ticks}");
                Harmony.PatchAll();

                NetworkUtils.LoadMirror();

                HookPatch.Enable();
                HookManager.RegisterAll();

                UpdateEvent.Initialize();

                foreach (var plugin in AssemblyLoader.InstalledPlugins)
                {
                    var pluginType = plugin._pluginType;
                    var pluginObj = plugin._plugin;

                    if (pluginObj != null && pluginObj == this)
                        continue;

                    Info("Extended Loader", $"Loading plugin '&2{plugin.PluginName}&r' by &1{plugin.PluginAuthor}&r ..");

                    HookManager.RegisterAll(pluginType, pluginObj);

                    foreach (var type in pluginType.Assembly.GetTypes())
                    {
                        if (type == pluginType)
                            continue;

                        HookManager.RegisterAll(type, null);
                    }

                    Info("Extended Loader", $"Loaded plugin '&2{plugin.PluginName}&r' by &1{plugin.PluginAuthor}&r!");
                }

                Info("Extended Loader", $"Finished loading!");
            }
            catch (Exception ex)
            {
                Error("Extended Loader", $"A general loading error has occured!\n{ex.ToColoredString()}");
            }

            _marker.MarkEnd();
            _marker.LogStats();
            _marker.Clear();
        }

        public static void Info(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Info(message.ToString(), source);
        }

        public static void Warn(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Warning(message.ToString(), source);
        }

        public static void Error(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Error(message.ToString(), source);
        }

        public static void Debug(string source, object message)
        {
            if (!CanDebug(source))
                return;

            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Debug(message.ToString(), source);
        }

        private static bool CanDebug(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                if (Loader.Config.Logging.DisabledSources.Contains(source))
                    return false;

                if (Loader.Config.Logging.EnabledSources.Contains(source))
                    return true;
            }

            return Loader.Config.Logging.DebugEnabled;
        }
    }
}