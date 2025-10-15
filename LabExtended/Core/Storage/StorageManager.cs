using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Events;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// Manages active storage instances.
    /// </summary>
    public static class StorageManager
    {
        /// <summary>
        /// Gets called once a player joins and their file storage is loaded.
        /// </summary>
        public static event Action<ExPlayer>? PlayerStorageLoaded;

        /// <summary>
        /// Gets called when another storage is loaded.
        /// </summary>
        public static event Action<StorageInstance>? OtherStorageLoaded;

        /// <summary>
        /// Gets called once the server's personal storage is loaded.
        /// </summary>
        public static event Action? ServerStorageLoaded;

        /// <summary>
        /// Gets called once the server's shared storage is loaded.
        /// </summary>
        public static event Action? SharedStorageLoaded;

        /// <summary>
        /// Gets the server's personal storage instance. Will be null if disabled in config!
        /// </summary>
        public static StorageInstance? ServerStorage { get; private set; }

        /// <summary>
        /// Gets the shared storage instance. Will be null if disabled in config!
        /// </summary>
        public static StorageInstance? SharedStorage { get; private set; }

        /// <summary>
        /// Creates and initializes a new <see cref="StorageInstance"/> with the specified name and configuration.
        /// </summary>
        /// <remarks>The storage path is determined based on the configuration settings in
        /// <c>ApiLoader.ApiConfig.StorageSection</c>.  If a custom path is defined for the specified <paramref
        /// name="name"/>, it will be used; otherwise, the path is  constructed using the base path (shared or
        /// server-specific) and the storage name.</remarks>
        /// <param name="name">The name of the storage instance. This value is used to determine the storage path.  Cannot be <see
        /// langword="null"/> or empty.</param>
        /// <param name="shared">A value indicating whether the storage instance should use the shared storage path.  If <see
        /// langword="true"/>, the shared path is used if configured; otherwise, the server-specific path is used. 
        /// Defaults to <see langword="false"/>.</param>
        /// <returns>A fully initialized <see cref="StorageInstance"/> configured with the specified name and path.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="Exception">Thrown if storage is disabled in the configuration.</exception>
        public static StorageInstance CreateStorage(string name, bool shared = false)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (!ApiLoader.ApiConfig.StorageSection.IsEnabled)
                throw new Exception($"Storage is disabled in config!");

            var path = ApiLoader.ApiConfig.StorageSection.CustomPaths.TryGetValue(name, out var customPath)
                ? customPath
                : (shared
                      ? Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, $"{name}_Shared")
                      : Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, $"{name}_{Server.Port.ToString()}"));

            var storage = new StorageInstance() { Name = name, Path = path };

            storage.Initialize();

            OtherStorageLoaded?.Invoke(storage);
            return storage;
        }

        private static void Internal_Left(ExPlayer player)
        {
            player.FileStorage?.Destroy();
            player.FileStorage = null;
        }

        private static void Internal_Verified(ExPlayer player)
        {
            if (!ApiLoader.ApiConfig.StorageSection.LoadPlayerStorage
                || player.DoNotTrack)
                return;

            var playerStorage = new StorageInstance 
            { 
                Name = player.UserId, 
                Path = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, "Players", player.UserId) 
            };

            playerStorage.Initialize();
            player.FileStorage = playerStorage;

            PlayerStorageLoaded?.Invoke(player);
        }

        internal static void Internal_Init()
        {
            if (!ApiLoader.ApiConfig.StorageSection.IsEnabled)
                return;

            if (string.IsNullOrWhiteSpace(ApiLoader.ApiConfig.StorageSection.StoragePath))
            {
                ApiLog.Warn("StorageManager", $"Storage is enabled in config, but it's missing the root directory!");
                return;
            }

            var playersDir = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, "Players");
            var sharedDir = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, "Shared");
            var serverDir = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, Server.Port.ToString());

            if (!Directory.Exists(playersDir))
                Directory.CreateDirectory(playersDir);

            if (!Directory.Exists(sharedDir))
                Directory.CreateDirectory(sharedDir);

            if (!Directory.Exists(serverDir))
                Directory.CreateDirectory(serverDir);

            ServerStorage = new() { Name = "ServerSpecific", Path = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, Server.Port.ToString()) };
            ServerStorage.Initialize();

            ServerStorageLoaded?.Invoke();

            SharedStorage = new() { Name = "ServerShared", Path = Path.Combine(ApiLoader.ApiConfig.StorageSection.StoragePath, "Shared") };
            SharedStorage.Initialize();

            SharedStorageLoaded?.Invoke();

            ExPlayerEvents.Left += Internal_Left;
            ExPlayerEvents.Verified += Internal_Verified;
        }
    }
}