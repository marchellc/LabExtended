using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core.Configs;
using LabExtended.Core.Hooking;

using LabExtended.Patches.Functions;

using Serialization;

using System.Reflection;

using LabApi.Loader;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;

using NorthwoodLib.Pools;

namespace LabExtended.Core
{
    public class ApiLoader : Plugin
    {
        public ApiLoader()
        {
            Loader = this;
            LoaderPoint();
        }
        
        public const string LoadFinishedMessage = "[LOADER] Enabling all plugins";
        public const string LoaderName = "LabExtended";
        
        public static string DirectoryPath { get; private set; }

        public static string BaseConfigPath { get; private set; }
        public static string ApiConfigPath { get; private set; }

        public static BaseConfig BaseConfig { get; private set; }
        public static ApiConfig ApiConfig { get; private set; }
        
        public static ApiLoader Loader { get; private set; }

        public static string SerializedBaseConfig => YamlParser.Serializer.Serialize(BaseConfig ??= new BaseConfig());
        public static string SerializedApiConfig => YamlParser.Serializer.Serialize(ApiConfig ??= new ApiConfig());
        
        public override string Name { get; } = "LabExtended";
        public override string Author { get; } = "marchellcx";
        public override string Description { get; } = "An extended API for LabAPI.";
        
        public override Version Version => ApiVersion.Version;
        public override Version RequiredApiVersion => null;

        public override LoadPriority Priority { get; } = LoadPriority.Highest;

        public override void Enable() { }
        public override void Disable() { }
        
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
        
        // This method is invoked by the LogPatch when LabAPI logs it's "enabling all plugins" line.
        private static void LogPoint()
        {
            ApiLog.Info("Extended Loader", $"LabAPI has finished loading, registering plugin hooks.");

            LogPatch.OnLogging -= LogHandler;

            var loadedAssemblies = ListPool<Assembly>.Shared.Rent();
            
            foreach (var plugin in PluginLoader.Plugins.Keys)
            {
                try
                {
                    if (plugin is null)
                        continue;
                    
                    if (!string.IsNullOrWhiteSpace(plugin?.Name) && plugin.Name == LoaderName)
                        continue;

                    var type = plugin.GetType();
                    var assembly = type.Assembly;

                    HookManager.RegisterAll(type, plugin);

                    if (!loadedAssemblies.Contains(assembly))
                    {
                        loadedAssemblies.Add(assembly);

                        foreach (var asmType in assembly.GetTypes())
                        {
                            if (asmType == type)
                                continue;

                            HookManager.RegisterAll(asmType, null);
                        }
                    }

                    var loadMethod = type.FindMethod("ExtendedLoad");

                    if (loadMethod != null)
                    {
                        if (loadMethod.IsStatic)
                            loadMethod.Invoke(null, null);
                        else
                            loadMethod.Invoke(plugin, null);
                    }

                    ApiLog.Info("Extended Loader", $"Loaded plugin &3{plugin.Name}&r!");
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Extended Loader", $"Failed while loading plugin &3{plugin.Name}&r:\n{ex.ToColoredString()}");
                }
            }

            loadedAssemblies.ForEach(x => x.InvokeStaticMethods(y => y.HasAttribute<LoaderInitializeAttribute>(), y => y.GetCustomAttribute<LoaderInitializeAttribute>().Priority, false));

            ListPool<Assembly>.Shared.Return(loadedAssemblies);
            
            ApiPatcher.ApplyPatches(typeof(ApiLoader).Assembly);

            typeof(ApiLoader).Assembly.InvokeStaticMethods(x => x.HasAttribute<LoaderInitializeAttribute>(), x => x.GetCustomAttribute<LoaderInitializeAttribute>().Priority, false);

            ApiLog.Info("Extended Loader", $"Loading finished!");
        }
        
        // This method is invoked by the loader.
        private static void LoaderPoint()
        {
            ApiLog.Info("Extended Loader", $"Loading version &1{ApiVersion.Version}&r ..");

            DirectoryPath = Path.Combine(PathManager.Plugins.FullName, "LabExtended");

            BaseConfigPath = Path.Combine(DirectoryPath, "config.yml");
            ApiConfigPath = Path.Combine(DirectoryPath, "api_config.yml");

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            LoadConfig();
            SaveConfig();

            ApiLog.Info("Extended Loader", $"Config files have been loaded.");

            if (!ApiVersion.CheckCompatibility())
                return;
            
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