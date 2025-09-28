using System.Collections.Concurrent;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events.Player;

namespace LabExtended.Utilities.FileStorage;

/// <summary>
/// Manages file storages.
/// </summary>
public static class FileStorageManager
{
    private static volatile FileSystemWatcher watcher;
    private static volatile FileStorageInstance serverInstance;
    
    private static volatile ConcurrentDictionary<string, FileStorageInstance> instances = new();
    internal static volatile ConcurrentDictionary<string, FileStorageComponent> pathToComponent = new();

    internal static event Action? CheckDirtiness;

    /// <summary>
    /// How many milliseconds a component's write watch must have elapsed to not be ignored.
    /// </summary>
    public const int WriteWatchIgnore = 100;
    
    /// <summary>
    /// Gets called when a player's file storage is loaded.
    /// </summary>
    public static event Action<ExPlayer>? OnLoaded; 
    
    /// <summary>
    /// Gets called when the server's file storage is loaded.
    /// </summary>
    public static event Action? OnServerLoaded; 
    
    /// <summary>
    /// Gets the path to the storage directory.
    /// </summary>
    public static string DirectoryPath { get; private set; } = string.Empty;

    /// <summary>
    /// A list of all found component types.
    /// </summary>
    public static HashSet<Type> Components { get; } = new();

    /// <summary>
    /// Gets the file storage instance targeting the dedicated server player.
    /// </summary>
    public static FileStorageInstance Server => serverInstance;

    /// <summary>
    /// Gets the path to the storage directory of a specific user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>the path string</returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetPlayerDirectory(string userId)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        
        var path = Path.Combine(DirectoryPath, userId);
        
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        
        return path;
    }

    /// <summary>
    /// Gets an active storage instance.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <returns>the found storage instance, otherwise null</returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static FileStorageInstance? GetInstance(string userId)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (!instances.TryGetValue(userId, out var instance))
            return null;

        return instance;
    }

    /// <summary>
    /// Gets an active storage component.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <param name="componentType">The target component type.</param>
    /// <returns>the found component instance, otherwise null</returns>
    public static FileStorageComponent? GetComponent(string userId, Type componentType)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));
        
        if (!instances.TryGetValue(userId, out var instance))
            return null;

        if (!instance.Components.TryGetValue(componentType, out var component))
            return null;

        return component;
    }
    
    /// <summary>
    /// Gets an active storage component.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <param name="componentType">The target component type.</param>
    /// <returns>the found component instance, otherwise null</returns>
    public static T? GetComponent<T>(string userId, Type componentType) where T : FileStorageComponent
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));
        
        if (!instances.TryGetValue(userId, out var instance))
            return null;

        if (!instance.Components.TryGetValue(componentType, out var component))
            return null;

        if (component is not T target)
            return null;

        return target;
    }

    /// <summary>
    /// Gets a value of an active property.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <param name="propertyName">The target property name.</param>
    /// <param name="componentType">The target component type.</param>
    /// <param name="defaultValue">The default value to return.</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>Property value if found, otherwise the defaultValue parameter.</returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static T? GetValue<T>(string userId, string propertyName, Type componentType, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));
        
        if (!instances.TryGetValue(userId, out var instance))
            return defaultValue;

        if (!instance.Components.TryGetValue(componentType, out var component))
            return defaultValue;

        if (!component.properties.TryGetValue(propertyName, out var property))
            return defaultValue;

        if (property.NonGenericValue is not T target)
            return defaultValue;

        return target;
    }
    
    /// <summary>
    /// Gets a value of an active property.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <param name="propertyName">The target property name.</param>
    /// <param name="componentType">The target component type.</param>
    /// <param name="value">The retrieved property value.</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>true if the property was found</returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetValue<T>(string userId, string propertyName, Type componentType, out T value)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            throw new Exception("Base directory path has not been set.");
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        if (componentType is null)
            throw new ArgumentNullException(nameof(componentType));

        value = default;
        
        if (!instances.TryGetValue(userId, out var instance))
            return false;

        if (!instance.Components.TryGetValue(componentType, out var component))
            return false;

        if (!component.properties.TryGetValue(propertyName, out var property))
            return false;

        if (property.NonGenericValue is not T target)
            return false;

        value = target;
        return true;
    }

    private static void LoadInstances()
    {
        foreach (var directory in Directory.GetDirectories(DirectoryPath, "*@steam"))
        {
            var userId = Path.GetFileName(directory);

            ApiLog.Debug("File Storage", $"Loading user ID &6{userId}&r");

            var instance = new FileStorageInstance(directory);
            
            instance.userId = userId;
            instance.Load();

            instances.TryAdd(userId, instance);

            if (string.Equals(userId, "Server@steam", StringComparison.InvariantCulture))
                serverInstance = instance;

            ApiLog.Debug("File Storage", $"Loaded storage for user ID &6{instance.UserId}&r");
        }
    }

    private static void LoadWatcher()
    {
        if (watcher is null)
        {
            watcher = new(DirectoryPath)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes
            };
            
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
        }
    }

    private static void OnChanged(object _, FileSystemEventArgs ev)
    {
        if (!pathToComponent.TryGetValue(ev.FullPath, out var targetComponent))
            return;

        ApiLog.Debug("File Storage", $"Component &3{targetComponent.Name}&r file changed (UID: {targetComponent.Storage.UserId}, WriteWatch: {targetComponent.writeWatch.ElapsedMilliseconds} / {WriteWatchIgnore})");

        if (targetComponent.writeWatch.ElapsedMilliseconds < WriteWatchIgnore)
            return;
        
        Task.Run(() => targetComponent.ReloadFile())
            .ContinueWithOnMain(_ => targetComponent.OnReloaded());
    }

    private static void OnHostLeft(ExPlayer player)
    {
        if (serverInstance != null)
        {
            serverInstance.userId = "Server@steam";
            
            serverInstance.IsActive = false;
            serverInstance.OnLeft();

            serverInstance.Player = null;
            
            ApiLog.Debug("File Storage", $"Removed server player storage");
        }
    }

    private static void OnHostJoined(ExPlayer player)
    {
        if (serverInstance != null)
        {
            serverInstance.userId = "Server@steam";
            
            serverInstance.Player = player;
            serverInstance.OnJoined();
         
            serverInstance.IsActive = true;
            
            ApiLog.Debug("File Storage", $"Loaded server player storage");
            
            OnServerLoaded?.InvokeSafe();
        }
        else
        {
            var path = Path.Combine(DirectoryPath, "Server@steam");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            serverInstance = new(path);
            serverInstance.userId = "Server@steam";
            
            serverInstance.Load();

            serverInstance.Player = player;
            serverInstance.OnJoined();

            serverInstance.IsActive = true;
            
            instances.TryAdd("Server@steam", serverInstance);
            
            ApiLog.Debug("File Storage", $"Created server player storage");
            
            OnServerLoaded?.InvokeSafe();
        }
    }

    private static void OnVerified(ExPlayer player)
    {
        if (instances.TryGetValue(player.UserId, out var activeInstance))
        {
            activeInstance.IsActive = true;
            activeInstance.userId = player.UserId;
            
            activeInstance.Player = player;
            activeInstance.OnJoined();
            
            player.FileStorage = activeInstance;
            
            ApiLog.Debug("File Storage", $"Loaded active storage instance of player &3{player.Nickname}&r (&6{player.UserId}&r)");
            
            OnLoaded?.InvokeSafe(player);
        }
        else
        {
            var path = GetPlayerDirectory(player.UserId);
            var instance = new FileStorageInstance(path);

            instance.userId = player.UserId;
            instance.Load();
            
            instance.Player = player;
            instance.OnJoined();
            
            instance.IsActive = true;
            
            instances.TryAdd(player.UserId, instance);
        
            player.FileStorage = instance;
            
            ApiLog.Debug("File Storage", $"Created new storage instance of player &3{player.Nickname}&r (&6{player.UserId}&r)");
        
            OnLoaded?.InvokeSafe(player);
        }
    }

    private static void OnLeaving(PlayerLeavingEventArgs args)
    {
        if (args.Player.FileStorage != null)
        {
            args.Player.FileStorage.IsActive = false;
            args.Player.FileStorage.OnLeft();

            args.Player.FileStorage.Player = null;
            args.Player.FileStorage = null;
            
            ApiLog.Debug("File Storage", $"Removed storage instance of player &3{args.Player.Nickname}&r (&6{args.Player.UserId}&r)");
        }
    }

    private static void OnDiscovered(Type type)
    {
        if (!type.IsSubclassOf(typeof(FileStorageComponent)))
            return;

        if (type.HasAttribute<LoaderIgnoreAttribute>())
            return;
            
        Components.Add(type);
            
        ApiLog.Debug("File Storage", $"Found component &3{type.FullName}&r");
    }
    
    internal static void Internal_Init()
    {
        var config = ApiLoader.ApiConfig.StorageSection;
        
        if (!config.IsEnabled)
            return;

        if (string.IsNullOrWhiteSpace(config.Path))
        {
            config.Path = Path.Combine(ApiLoader.DirectoryPath, "Storage");
            
            ApiLoader.SaveConfig();
        }
        
        DirectoryPath = config.Path;
        
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        ReflectionUtils.Discovered += OnDiscovered;
        
        LoadInstances();
        LoadWatcher();
        
        InternalEvents.OnPlayerVerified += OnVerified;

        InternalEvents.OnHostJoined += OnHostJoined;
        InternalEvents.OnHostLeft += OnHostLeft;
        
        ExPlayerEvents.Leaving += OnLeaving;

        Task.Run(CheckDirtinessAsync);
    }

    private static async Task CheckDirtinessAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(100);
                
                CheckDirtiness?.InvokeSafe();
            }
            catch
            {
                // ignored
            }
        }
    }
}