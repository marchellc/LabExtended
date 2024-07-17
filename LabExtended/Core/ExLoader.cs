using HarmonyLib;
using LabExtended.API.Modules;
using LabExtended.Core.Hooking;
using LabExtended.Core.Logging;

using LabExtended.Extensions;
using LabExtended.Patches.Functions;
using LabExtended.Ticking;
using LabExtended.Utilities;

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Loader;

namespace LabExtended.Core
{
    /// <summary>
    /// The main class, used as a loader. Can also be used for modules.
    /// </summary>
    public class ExLoader : Module
    {
        /// <summary>
        /// Gets the server's version.
        /// </summary>
        public static Version GameVersion { get; } = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

        /// <summary>
        /// Gets the API version.
        /// </summary>
        public static Version ApiVersion { get; } = new Version(1, 0, 0);

        /// <summary>
        /// Gets the active <see cref="ExLoaderConfig"/> instance.
        /// </summary>
        public static ExLoaderConfig LoaderConfig => Loader?.Config;

        /// <summary>
        /// Gets the active <see cref="ExLoader"/> instance.
        /// </summary>
        public static ExLoader Loader { get; private set; }

        /// <summary>
        /// Gets the loader's plugin folder.
        /// </summary>
        public static string Folder => Loader.Handler.PluginDirectoryPath;

        /// <summary>
        /// Creates a new <see cref="ExLoader"/> instance. This is included only to implement the base constructor of <see cref="ModuleParent"/>.
        /// </summary>
        public ExLoader() : base() { }

        /// <summary>
        /// The plugin's config instance.
        /// </summary>
        [PluginConfig]
        public ExLoaderConfig Config;

        /// <summary>
        /// The plugin's handler instance.
        /// </summary>
        public PluginHandler Handler;

        /// <summary>
        /// The <see cref="HarmonyLib.Harmony"/> instance.
        /// </summary>
        public Harmony Harmony;

        /// <summary>
        /// Gets the loader's server version compatibility.
        /// </summary>
        public readonly VersionRange? GameCompatibility = new VersionRange(new Version(13, 5, 0));

        /// <summary>
        /// Loads the plugin.
        /// </summary>
        [PluginEntryPoint("LabExtended", "1.0.0", "An extension to NW's Plugin API.", "marchellc")]
        [PluginPriority(LoadPriority.Lowest)]
        public void Load()
        {
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

                Harmony = new Harmony($"com.extended.loader.{DateTime.Now.Ticks}");
                Harmony.PatchAll();

                NetworkUtils.LoadMirror();

                HookPatch.Enable();
                HookManager.RegisterAll();

                TickManager.Init();

                StartModule();
                AddCachedModules();

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
        }

        /// <summary>
        /// Unloads the plugin.
        /// </summary>
        [PluginUnload]
        public void Unload()
        {
            Info("Extended Loader", "Unloading ..");

            Harmony.UnpatchAll();
            Harmony = null;

            StopModule();

            TickManager.Kill();

            HookManager._activeDelegates.Clear();
            HookManager._activeHooks.Clear();

            Loader = null;
            Handler = null;

            Info("Extended Loader", $"Unloaded.");
        }

        /// <summary>
        /// Logs a new message with the INFO tag to the server console.
        /// </summary>
        /// <param name="source">Name of the source.</param>
        /// <param name="message">The message</param>
        public static void Info(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Info(message.ToString(), source);
        }

        /// <summary>
        /// Logs a new message with the WARN tag to the server console.
        /// </summary>
        /// <param name="source">Name of the source.</param>
        /// <param name="message">The message</param>
        public static void Warn(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Warning(message.ToString(), source);
        }

        /// <summary>
        /// Logs a new message with the ERROR tag to the server console.
        /// </summary>
        /// <param name="source">Name of the source.</param>
        /// <param name="message">The message</param>
        public static void Error(string source, object message)
        {
            if (message is Exception ex)
                message = ex.ToColoredString();

            Log.Error(message.ToString(), source);
        }

        /// <summary>
        /// Logs a new message with the DEBUG tag to the server console.
        /// </summary>
        /// <param name="source">Name of the source.</param>
        /// <param name="message">The message</param>
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
            if (Loader is null || Loader.Config is null)
                return true;

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