using LabExtended.Attributes;

using LabExtended.Core.Configs;
using LabExtended.Core.Hooking;

using LabExtended.Extensions;

using LabExtended.Patches.Functions;

using PluginAPI.Helpers;
using PluginAPI.Loader;

using Serialization;

using System.Reflection;

namespace LabExtended.Core
{
    public static class ApiLoader
    {
        public const string LoadFinishedMessage = "<---<    Plugin system is ready !    <---<";
        public const string LoaderName = "LabExtended Loader";

        public static string DirectoryPath { get; private set; }

        public static string BaseConfigPath { get; private set; }
        public static string ApiConfigPath { get; private set; }

        public static BaseConfig BaseConfig { get; private set; }
        public static ApiConfig ApiConfig { get; private set; }

        public static string SerializedBaseConfig => YamlParser.Serializer.Serialize(BaseConfig ??= new BaseConfig());
        public static string SerializedApiConfig => YamlParser.Serializer.Serialize(ApiConfig ??= new ApiConfig());

        public static void LoadConfig()
        {
            try
            {
                if (!File.Exists(BaseConfigPath))
                    File.WriteAllText(BaseConfigPath, SerializedBaseConfig);
                else
                    BaseConfig = YamlParser.Deserializer.Deserialize<BaseConfig>(File.ReadAllText(BaseConfigPath));

                if (!File.Exists(ApiConfigPath))
                    File.WriteAllText(ApiConfigPath, SerializedApiConfig);
                else
                    ApiConfig = YamlParser.Deserializer.Deserialize<ApiConfig>(File.ReadAllText(ApiConfigPath));
            }
            catch (Exception ex)
            {
                ApiLog.Error("Extended Loader", $"Failed to load config files due to an exception:\n{ex.ToColoredString()}");
            }
        }

        public static void SaveConfig()
        {
            try
            {
                File.WriteAllText(BaseConfigPath, SerializedBaseConfig);
                File.WriteAllText(ApiConfigPath, SerializedApiConfig);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Extended Loader", $"Failed to save config files due to an exception:\n{ex.ToColoredString()}");
            }
        }

        // This method is invoked by PluginAPI's plugin load finish log.
        private static void LogPoint()
        {
            ApiLog.Info("Extended Loader", $"PluginAPI has finished loading, registering plugin hooks.");

            LogPatch.OnLogging -= LogHandler;

            foreach (var handler in AssemblyLoader.InstalledPlugins)
            {
                try
                {
                    if (handler.PluginName == LoaderName)
                        continue;

                    HookManager.RegisterAll(handler._pluginType, handler._plugin);
                    HookManager.RegisterAll(handler._pluginType.Assembly);

                    var loadMethod = handler._pluginType.FindMethod("ExtendedLoad");

                    if (loadMethod != null)
                    {
                        if (loadMethod.IsStatic)
                            loadMethod.Invoke(null, null);
                        else
                            loadMethod.Invoke(handler._plugin, null);
                    }

                    ApiLog.Info("Extended Loader", $"Loaded plugin &3{handler.PluginName}&r!");
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Extended Loader", $"Failed while loading plugin &3{handler.PluginName}&r:\n{ex.ToColoredString()}");
                }
            }

            AssemblyLoader.InstalledPlugins.ForEach(x => x._pluginType.Assembly.InvokeStaticMethods(m => m.HasAttribute<LoaderCallbackAttribute>()));

            ApiPatcher.ApplyPatches(typeof(ApiLoader).Assembly);

            typeof(ApiLoader).Assembly.InvokeStaticMethods(x => x.HasAttribute<LoaderInitializeAttribute>(), x => x.GetCustomAttribute<LoaderInitializeAttribute>().Priority, false);

            ApiLog.Info("Extended Loader", $"Loading finished!");
        }

        // This method is invoked by the loader.
        private static void LoaderPoint()
        {
            ApiLog.Info("Extended Loader", $"Loading version &1{ApiVersion.Version}&r ..");

            DirectoryPath = Path.Combine(Paths.LocalPlugins.Plugins, "LabExtended");

            BaseConfigPath = Path.Combine(DirectoryPath, "config.yml");
            ApiConfigPath = Path.Combine(DirectoryPath, "api_config.yml");

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            LoadConfig();
            SaveConfig();

            ApiLog.Info("Extended Loader", $"Config files have been loaded.");

            if (!ApiVersion.CheckCompatibility())
                return;

            ApiCommands.InternalRegisterCommands();
            HookManager.RegisterAll(typeof(ApiLoader).Assembly);

            ApiLog.Info("Extended Loader", $"Waiting for PluginAPI ..");

            LogPatch.OnLogging += LogHandler;
            LogPatch.Enable();
        }

        private static void LogHandler(string logMessage)
        {
            if (logMessage is null || !logMessage.EndsWith(LoadFinishedMessage))
                return;

            LogPoint();
        }
    }
}