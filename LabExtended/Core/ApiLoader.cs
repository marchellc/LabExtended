using CommandSystem;

using HarmonyLib;
using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.API.Modules;
using LabExtended.Attributes;
using LabExtended.Core.Configs;
using LabExtended.Core.Configs.Api;
using LabExtended.Core.Hooking;

using LabExtended.Extensions;
using LabExtended.Patches.Functions;
using LabExtended.Utilities;

using PluginAPI.Core;
using PluginAPI.Helpers;
using PluginAPI.Loader;

using RemoteAdmin;

using Serialization;

using System.Reflection;

namespace LabExtended.Core
{
    /// <summary>
    /// The main class, used as a loader. Can also be used for modules.
    /// </summary>
    public class ApiLoader : API.Modules.Module
    {
        public static ApiOptions ApiOptions => Config?.ApiOptions;
        public static LogOptions LogOptions => Config?.LogOptions;
        public static VoiceOptions VoiceOptions => Config?.VoiceOptions;
        public static RoundOptions RoundOptions => Config?.RoundOptions;
        public static PerformanceOptions PerformanceOptions => Config?.ApiOptions?.PerformanceOptions;
        public static ThreadedVoiceOptions ThreadedVoiceOptions => Config?.VoiceOptions?.ThreadedVoiceOptions;
        public static PositionSynchronizerOptions PositionSynchronizerOptions => Config?.ApiOptions?.PositionSynchronizerOptions;

        public const int ApiMajor = 1;
        public const int ApiMinor = 0;
        public const int ApiBuild = 0;
        public const int ApiPatch = 0;

        /// <summary>
        /// Gets the server's version.
        /// </summary>
        public static Version GameVersion { get; } = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

        /// <summary>
        /// Gets the API version.
        /// </summary>
        public static Version ApiVersion { get; } = new Version(ApiMajor, ApiMinor, ApiBuild, ApiPatch);

        /// <summary>
        /// Gets the API's Assembly.
        /// </summary>
        public static Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets the active <see cref="ApiLoader"/> instance.
        /// </summary>
        public static ApiLoader Loader { get; private set; }

        /// <summary>
        /// Gets the API's plugin folder.
        /// </summary>
        public static string Folder { get; private set; }

        /// <summary>
        /// Gets the API's config file path.
        /// </summary>
        public static string ConfigPath { get; private set; }

        /// <summary>
        /// The <see cref="HarmonyLib.Harmony"/> instance.
        /// </summary>
        public static volatile Harmony Harmony;

        /// <summary>
        /// The <see cref="Configs.Config"/> instance.
        /// </summary>
        public static volatile Config Config;

        /// <summary>
        /// Gets the API's server version compatibility.
        /// </summary>
        public static readonly VersionRange? GameCompatibility = new VersionRange(new Version(13, 5, 1));

        private bool _pluginsLoaded = false;

        /// <summary>
        /// Loads all plugins.
        /// </summary>
        public void LoadPlugins()
        {
            if (_pluginsLoaded)
                return;

            try
            {
                typeof(ApiLoader).Assembly.InvokeStaticMethods(m => m.HasAttribute<OnLoadAttribute>());

                foreach (var type in typeof(ApiLoader).Assembly.GetTypes())
                {
                    if (type.IsClass && typeof(ICommand).IsAssignableFrom(type))
                    {
                        ICommand command = null;

                        foreach (var commandAttribute in type.GetCustomAttributesData())
                        {
                            if (commandAttribute.AttributeType != typeof(CommandHandlerAttribute))
                                continue;

                            command ??= (ICommand)Activator.CreateInstance(type);

                            InternalRegisterCommand(type, (Type)commandAttribute.ConstructorArguments[0].Value, command);
                        }
                    }
                }

                HookManager.RegisterAll();

                Loader.StartModule();

                Loader.AddCachedModules();
                Loader.AddModule<GlobalHintModule>(false);

                foreach (var plugin in AssemblyLoader.InstalledPlugins)
                {
                    var pluginType = plugin._pluginType;
                    var pluginObj = plugin._plugin;

                    Info("Extended Loader", $"Loading plugin '&2{plugin.PluginName}&r' by &1{plugin.PluginAuthor}&r ..");

                    HookManager.RegisterAll(pluginType, pluginObj);

                    foreach (var type in pluginType.Assembly.GetTypes())
                    {
                        type.InvokeStaticMethod(m => m.HasAttribute<OnLoadAttribute>());

                        if (type == pluginType)
                            continue;

                        HookManager.RegisterAll(type, null);
                    }

                    pluginType.Assembly.InvokeStaticMethods(m => m.HasAttribute<HookCallbackAttribute>());

                    Info("Extended Loader", $"Loaded plugin '&2{plugin.PluginName}&r' by &1{plugin.PluginAuthor}&r!");
                }

                foreach (var plugin in AssemblyLoader.Plugins.Keys)
                    plugin.InvokeStaticMethods(m => m.HasAttribute<LoaderCallbackAttribute>());

                _pluginsLoaded = true;

                LogPatch.OnLogging -= InternalHandleLog;
            }
            catch (Exception ex)
            {
                Error("Extended Loader", $"Failed to load plugin(s)!\n{ex.ToColoredString()}");
            }
        }

        /// <summary>
        /// Loads the API.
        /// </summary>
        public static void Load()
        {
            if (Loader != null)
                throw new Exception($"API has already been loaded.");

            try
            {
                Info("Extended Loader", $"Loading version &2{ApiVersion}&r ..");

                if (GameCompatibility.HasValue && !GameCompatibility.Value.InRange(GameVersion))
                {
                    Error("Extended Loader", $"Attempted to load for an unsupported game version (&1{GameVersion}&r) - supported: &2{GameCompatibility.Value}&r");
                    return;
                }

                Assembly = Assembly.GetExecutingAssembly();

                Folder = $"{Paths.LocalPlugins.Plugins}/LabExtended";
                ConfigPath = $"{Folder}/config.yml";

                if (!Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);

                LoadConfig();

                if (Config is null)
                {
                    Error("Extended Loader", $"Failed to load the configuration file.");
                    return;
                }

                Loader = new ApiLoader();

                Harmony = new Harmony($"com.extended.loader.{DateTime.Now.Ticks}");
                Harmony.PatchAll();

                if (LogOptions != null && LogOptions.DebugEnabled)
                    ServerConsole.AddLog("\n" + YamlParser.Serializer.Serialize(Config), ConsoleColor.Cyan);

                LogPatch.OnLogging += InternalHandleLog;
            }
            catch (Exception ex)
            {
                Error("Extended Loader", $"A general loading error has occurred!\n{ex.ToColoredString()}");
            }
        }

        public static void RequireCameraNpc()
        {
            if (!ApiOptions.EnableCameraNpc)
            {
                Warn("API", $"A plugin has requested to enable the camera NPC for SCP-079 cameras, but this feature is disabled in config! (&6enable_camera_npc&r)");
                return;
            }

            if (Camera.m_Required)
                return;

            Camera.m_Required = true;

            Debug("API", $"A plugin has enabled the camera NPC feature.");
        }

        /// <summary>
        /// Loads config from the config file.
        /// </summary>
        public static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                SaveConfig();
                return;
            }

            Config = YamlParser.Deserializer.Deserialize<Config>(File.ReadAllText(ConfigPath));
        }

        /// <summary>
        /// Saves the config file.
        /// </summary>
        public static void SaveConfig()
        {
            Config ??= new Config();

            File.WriteAllText(ConfigPath, YamlParser.Serializer.Serialize(Config));
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

            ServerConsole.AddLog(Log.FormatText($"&7[&b&5Debug&B&7] &7[&b&2{source}&B&7]&r {message}", "7", false), ConsoleColor.Magenta);
        }

        private static bool CanDebug(string source)
        {
            if (Config is null || Config.LogOptions is null)
                return true;

            if (!string.IsNullOrWhiteSpace(source))
            {
                if (Config.LogOptions.DisabledSources.Contains(source))
                    return false;

                if (Config.LogOptions.EnabledSources.Contains(source))
                    return true;
            }

            return Config.LogOptions.DebugEnabled;
        }

        private static void InternalHandleLog(string log)
        {
            if (Loader is null || Loader._pluginsLoaded || !log.EndsWith("<---<    Plugin system is ready !    <---<"))
                return;

            Loader.LoadPlugins();
        }

        private static void InternalRegisterCommand(Type type, Type cmdType, ICommand command)
        {
            if (cmdType == typeof(GameConsoleCommandHandler))
                GameCore.Console.singleton.ConsoleCommandHandler.RegisterCommand(command);
            else if (cmdType == typeof(RemoteAdminCommandHandler))
                CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(command);
            else if (cmdType == typeof(ClientCommandHandler))
                QueryProcessor.DotCommandHandler.RegisterCommand(command);
        }
    }
}