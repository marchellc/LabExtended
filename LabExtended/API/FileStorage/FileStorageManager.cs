using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events.Player;
using LabExtended.Utilities.Testing.FileStorage;

namespace LabExtended.API.FileStorage;

/// <summary>
/// Manages file storages.
/// </summary>
public static class FileStorageManager
{
    /// <summary>
    /// Gets called when a player's file storage is loaded.
    /// </summary>
    public static event Action<ExPlayer>? OnLoaded; 
    
    /// <summary>
    /// Gets the path to the storage directory.
    /// </summary>
    public static string DirectoryPath { get; private set; } = string.Empty;

    /// <summary>
    /// A list of all found component types.
    /// </summary>
    public static HashSet<Type> Components { get; } = new();

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

    private static void OnVerified(ExPlayer player)
    {
        var path = GetPlayerDirectory(player.UserId);
        var instance = new FileStorageInstance(player, path);
        
        instance.Load();
        
        player.FileStorage = instance;
        
        OnLoaded?.InvokeSafe(player);
    }

    private static void OnLeaving(PlayerLeavingEventArgs args)
    {
        if (args.Player.FileStorage != null)
        {
            args.Player.FileStorage.Dispose();
            args.Player.FileStorage = null;
        }
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
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
        
        TypeExtensions.ForEachLoadedType(type =>
        {
            if (!type.IsSubclassOf(typeof(FileStorageComponent)))
                return;

            if (type == typeof(TestFileStorageComponent) && !TestFileStorageComponent.IsEnabled)
                return;
            
            Components.Add(type);
        });

        if (Components.Count < 1)
            return;
        
        InternalEvents.OnPlayerVerified += OnVerified;
        ExPlayerEvents.Leaving += OnLeaving;
    }
}