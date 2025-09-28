using System.Reflection;

using CommandSystem.Commands.Shared;

using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Loader.Features.Yaml;

using LabExtended.Events;
using LabExtended.Commands;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core.Configs;

using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using Version = System.Version;
using LabExtended.API.Audio;
using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.API.CustomFirearms;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomRoles;
using LabExtended.API.CustomTeams;
using LabExtended.API.FileStorage;
using LabExtended.API.Hints;
using LabExtended.API.Images;
using LabExtended.API.RemoteAdmin.Actions;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Settings;
using LabExtended.API.Toys;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Parameters;
using LabExtended.Patches.Events.Scp049;
using LabExtended.Patches.Fixes;
using LabExtended.Patches.Functions;
using LabExtended.Utilities.Firearms;
using LabExtended.Utilities.Unity;
using LabExtended.API.Custom.Effects;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).

namespace LabExtended.Core;

/// <summary>
/// Responsible for loading LabExtended.
/// </summary>
public class ApiLoader : Plugin
{
    /// <summary>
    /// Initializes a new loader instance.
    /// </summary>
    public ApiLoader()
    {
        Loader = this;
        LoaderPoint();
    }

    /// <summary>
    /// The message that LabAPI prints once it starts enabling plugins.
    /// </summary>
    public const string LoadFinishedMessage = "[LOADER] Enabling all plugins";

    /// <summary>
    /// Gets the loader's assembly.
    /// </summary>
    public static Assembly Assembly { get; } = typeof(ApiLoader).Assembly;

    /// <summary>
    /// Gets the path to the LabExtended directory.
    /// </summary>
    public static string DirectoryPath { get; private set; }

    /// <summary>
    /// Gets the path to the base config file.
    /// </summary>
    public static string BaseConfigPath { get; private set; }

    /// <summary>
    /// Gets the path to the API config file.
    /// </summary>
    public static string ApiConfigPath { get; private set; }

    /// <summary>
    /// Gets the base config singleton.
    /// </summary>
    public static BaseConfig BaseConfig { get; private set; }

    /// <summary>
    /// Gets the API config singleton.
    /// </summary>
    public static ApiConfig ApiConfig { get; private set; }

    /// <summary>
    /// Gets the loader singleton.
    /// </summary>
    public static ApiLoader Loader { get; private set; }

    /// <summary>
    /// Gets the YAML-serialized string of <see cref="BaseConfig"/>.
    /// </summary>
    public static string SerializedBaseConfig => YamlConfigParser.Serializer.Serialize(BaseConfig ??= new());

    /// <summary>
    /// Gets the YAML-serialized string of <see cref="ApiConfig"/>.
    /// </summary>
    public static string SerializedApiConfig => YamlConfigParser.Serializer.Serialize(ApiConfig ??= new());

    /// <summary>
    /// Gets the loader's name.
    /// </summary>
    public override string Name { get; } = "LabExtended";

    /// <summary>
    /// Gets the loader's author.
    /// </summary>
    public override string Author { get; } = "marchellcx";

    /// <summary>
    /// Gets the loader's description.
    /// </summary>
    public override string Description { get; } = "An extended API for LabAPI.";

    /// <inheritdoc cref="Plugin.IsTransparent"/>
    public override bool IsTransparent => true;

    /// <summary>
    /// Gets the loader's current version.
    /// </summary>
    public override Version Version => ApiVersion.Version;

    /// <summary>
    /// Gets the loader's required LabAPI version.
    /// </summary>
    public override Version? RequiredApiVersion { get; } = null;

    /// <summary>
    /// Gets the loader's priority.
    /// </summary>
    public override LoadPriority Priority { get; } = LoadPriority.Highest;

    /// <summary>
    /// Dummy method.
    /// </summary>
    public override void Enable()
    {
        
    }

    /// <summary>
    /// Dummy method.
    /// </summary>
    public override void Disable()
    {
        
    }

    /// <summary>
    /// Loads both of loader's configs.
    /// </summary>
    public static void LoadConfig()
    {
        try
        {
            if (!File.Exists(BaseConfigPath))
                File.WriteAllText(BaseConfigPath!, SerializedBaseConfig);
            else
                BaseConfig = YamlConfigParser.Deserializer.Deserialize<BaseConfig>(File.ReadAllText(BaseConfigPath!));

            if (!File.Exists(ApiConfigPath))
                File.WriteAllText(ApiConfigPath!, SerializedApiConfig);
            else
                ApiConfig = YamlConfigParser.Deserializer.Deserialize<ApiConfig>(File.ReadAllText(ApiConfigPath!));

            ApiLog.IsTrueColorEnabled = BaseConfig?.TrueColorEnabled ?? true;
            ApiPatcher.TranspilerDebug = BaseConfig?.TranspilerDebugEnabled ?? false;
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Failed to load config files due to an exception:\n{ex.ToColoredString()}");
        }
    }

    /// <summary>
    /// Saves both of loader's configs.
    /// </summary>
    public static void SaveConfig()
    {
        try
        {
            File.WriteAllText(BaseConfigPath, SerializedBaseConfig);
            File.WriteAllText(ApiConfigPath, SerializedApiConfig);
        }
        catch (Exception ex)
        {
            ApiLog.Error("LabExtended", $"Failed to save config files due to an exception:\n{ex.ToColoredString()}");
        }
    }

    // This method is invoked by the LogPatch when LabAPI logs it's "enabling all plugins" line.
    private static void LogPoint()
    {
        ApiLog.Info("LabExtended", "LabAPI has finished loading, registering plugin hooks.");

        ExServerEvents.Logging -= Internal_Log;

        var loadedAssemblies = ListPool<Assembly>.Shared.Rent();

        foreach (var plugin in PluginLoader.Plugins.Keys)
        {
            try
            {
                if (plugin is null)
                    continue;

                if (Loader != null && plugin == Loader)
                    continue;

                var type = plugin.GetType();
                var assembly = type.Assembly;

                if (!loadedAssemblies.Contains(assembly))
                {
                    loadedAssemblies.Add(assembly);

                    assembly.RegisterUpdates();
                    assembly.RegisterCommands();

                    if (type.HasAttribute<LoaderPatchAttribute>())
                        assembly.ApplyPatches();
                }

                var loadMethod = type.FindMethod("ExtendedLoad");

                loadMethod?.Invoke(loadMethod.IsStatic ? null : plugin, null);

                ApiLog.Info("LabExtended", $"Loaded plugin &3{plugin.Name}&r!");
            }
            catch (Exception ex)
            {
                ApiLog.Error("LabExtended", $"Failed while loading plugin &3{plugin.Name}&r:\n{ex.ToColoredString()}");
            }
        }

        try
        {
            loadedAssemblies.ForEach(x => x.InvokeStaticMethods(
                y => y.HasAttribute<LoaderInitializeAttribute>(out var attribute) && attribute.Priority >= 0,
                y => y.GetCustomAttribute<LoaderInitializeAttribute>().Priority, false));
        }
        catch
        {
            // ignored, logged by the extension
        }

        ListPool<Assembly>.Shared.Return(loadedAssemblies);

        Assembly.ApplyPatches();

        InvokeApi(false);

        ReflectionUtils.Load();

        ApiLog.Info("LabExtended", "Loading finished!");
    }

    // This method is invoked by the loader.
    private static void LoaderPoint()
    {
        ApiLog.Info("LabExtended", $"Loading version &1{ApiVersion.Version}&r ..");

        DirectoryPath = Loader.GetConfigDirectory(StartupArgs.Args.Any(x => x.Contains("LabExGlobal"))).FullName;

        BaseConfigPath = Path.Combine(DirectoryPath, "config.yml");
        ApiConfigPath = Path.Combine(DirectoryPath, "api_config.yml");

        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        LoadConfig();
        SaveConfig();

        ApiLog.Info("LabExtended", "Config files have been loaded.");

        if (!ApiVersion.CheckCompatibility())
            return;

        if (!string.IsNullOrWhiteSpace(BuildInfoCommand.ModDescription))
            BuildInfoCommand.ModDescription += $"\nLabExtended v{ApiVersion.Version}";
        else
            BuildInfoCommand.ModDescription = $"\nLabExtended v{ApiVersion.Version}";

        ExServerEvents.Logging += Internal_Log;
        ExServerEvents.Quitting += Internal_Quit;

        Assembly.RegisterUpdates();
        Assembly.RegisterCommands();

        InvokeApi(true);

        ApiLog.Info("LabExtended", "Waiting for LabAPI ..");
    }

    private static void InvokeApi(bool isPreload)
    {
        if (isPreload)
        {
            SwitchContainer.Internal_Init();
            LabApiParentCommandFix.Internal_Init();
            LogPatch.Internal_Init();
        }
        else
        {
            PlayerLoopHelper.Internal_InitFirst();
            PlayerUpdateHelper.Internal_Init();

            ThreadUtils.Internal_Init();
            TimingUtils.Internal_Init();

            AudioManager.Internal_Init();
            Camera.Internal_Init();

            CustomPlayerEffect.Internal_Init();
            CustomFirearmEvents.Internal_Init();
            CustomItemRegistry.Internal_Init();
            CustomRoleManager.Internal_Init();
            CustomTeamHandler.Internal_Init();
            CustomTeamRegistry.Internal_Init();

            Elevator.Internal_Init();

            ExMap.Internal_Init();
            ExRound.Internal_Init();
            ExServer.Internal_Init();
            ExTeslaGate.Internal_Init();
            ExServerEvents.Internal_Init();

            FileStorageManager.Internal_Init();

            HintController.Internal_Init();

            ImageLoader.Internal_Init();
            ImageSpawner.Internal_Init();

            RemoteAdminActionProvider.Internal_Init();
            RemoteAdminController.Internal_Init();

            SettingsManager.Internal_Init();

            AdminToy.Internal_Init();

            CommandManager.Internal_Init();
            CommandParameterParserUtils.Internal_Init();
            CommandPropertyUtils.Internal_Init();

            InternalEvents.Internal_Init();

            Scp049CancellingResurrectionPatch.Internal_Init();

            FirearmModuleCache.Internal_Init();

            HttpUtils.Internal_Init();

            ItemTracker.Internal_Init();

            PositionSync.Internal_Init();
            RoleSync.Internal_Init();

            PlayerLoopHelper.Internal_InitLast(); // has to be last
        }
    }

    private static void Internal_Log(string logMessage)
    {
        if (logMessage is null || !logMessage.EndsWith(LoadFinishedMessage))
            return;

        LogPoint();
    }

    private static void Internal_Quit()
    {
        ExServerEvents.Quitting -= Internal_Quit;

        if (BaseConfig is null || !BaseConfig.UnloadPluginsOnQuit)
            return;

        foreach (var plugin in PluginLoader.Plugins.Keys)
        {
            if (plugin is null)
                continue;

            if (Loader != null && plugin == Loader)
                continue;

            ApiLog.Debug("LabExtended", $"Unloading plugin &6{plugin.Name}&r ..");

            try
            {
                plugin.UnregisterCommands();
                plugin.Disable();
            }
            catch (Exception ex)
            {
                ApiLog.Error("LabExtended", $"Could not unload plugin &1{plugin.Name}&r:\n{ex.ToColoredString()}");
            }
        }
    }
}