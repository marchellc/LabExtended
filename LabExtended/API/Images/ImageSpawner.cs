using LabExtended.API.Toys;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Configs.Sections;

using LabExtended.Events;
using LabExtended.Images.Playback;
using LabExtended.Utilities;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Images;

/// <summary>
/// Manages spawning custom images.
/// </summary>
public static class ImageSpawner
{
    /// <summary>
    /// Gets the image API config.
    /// </summary>
    public static ImageSection Config => ApiLoader.ApiConfig.ImageSection;

    /// <summary>
    /// Gets a list of all spawned images.
    /// </summary>
    public static List<TextToy> SpawnedImages { get; } = new();

    /// <summary>
    /// Spawns a text toy with a specified image and starts playing it.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="position">The toy position.</param>
    /// <param name="rotation">The toy rotation.</param>
    /// <returns>The spawned text toy instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    public static TextToy SpawnImage(this ImageFile image, Vector3 position, Quaternion rotation)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));

        if (image.IsDisposed)
            throw new ObjectDisposedException(nameof(image));

        var toy = new TextToy(position, rotation);

        toy.PlaybackDisplay.EnableOption(PlaybackFlags.Loop);
        toy.PlaybackDisplay.Play(image);

        return toy;
    }

    private static void OnStopped()
    {
        foreach (var spawnedImage in SpawnedImages)
        {
            if (spawnedImage?.Base != null)
            {
                NetworkServer.Destroy(spawnedImage.GameObject);
            }
        }
        
        SpawnedImages.Clear();
    }
    
    private static void OnStarted()
    {
        foreach (var spawnableImage in Config.SpawnImages)
        {
            try
            {
                if (string.Equals(spawnableImage.Name, "example"))
                    continue;
                
                if (spawnableImage.Chance == 0f)
                    continue;

                if (spawnableImage.Chance != 100f && !WeightUtils.GetBool(spawnableImage.Chance, false))
                    continue;

                if (!ImageLoader.TryGet(spawnableImage.Name, out var image))
                {
                    ApiLog.Warn("Image Spawner", $"Attempted to spawn image &3{spawnableImage.Name}&r, but it was not loaded.");
                    continue;
                }

                var toy = image.SpawnImage(spawnableImage.Position.Vector, spawnableImage.Rotation.Quaternion);

                if (toy?.Base != null)
                    SpawnedImages.Add(toy);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Image Spawner", $"Could not spawn image &3{spawnableImage.Name}&r:\n{ex}");
            }
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        InternalEvents.OnRoundStarted += OnStarted;
        InternalEvents.OnRoundEnded += OnStopped;
    }
}