using LabExtended.API.Images.Configs;
using LabExtended.API.Toys;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Configs.Sections;

using LabExtended.Events;
using LabExtended.Extensions;
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
    /// Gets called once an image is spawned.
    /// </summary>
    public static event Action<string, SpawnableImage, TextToy>? Spawned; 
    
    /// <summary>
    /// Gets the image API config.
    /// </summary>
    public static ImageSection Config => ApiLoader.ApiConfig.ImageSection;

    /// <summary>
    /// Gets a list of all spawned images.
    /// </summary>
    public static Dictionary<string, List<KeyValuePair<SpawnableImage, TextToy>>> SpawnedImages { get; } = new();

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
        foreach (var spawnedList in SpawnedImages)
        {
            foreach (var spawnedImage in spawnedList.Value)
            {
                if (spawnedImage.Value?.Base != null)
                {
                    NetworkServer.Destroy(spawnedImage.Value.GameObject);
                }
            }
        }
        
        SpawnedImages.Clear();
    }
    
    private static void OnStarted()
    {
        foreach (var spawnablePair in Config.SpawnImages)
        {
            if (spawnablePair.Key == "example")
                continue;
            
            if (SpawnedImages.TryGetValue(spawnablePair.Key, out var spawnedList) && spawnedList.Count > 0)
                continue;

            if (spawnedList is null)
                SpawnedImages.Add(spawnablePair.Key, spawnedList = new());

            foreach (var spawnableImage in spawnablePair.Value)
            {
                if (spawnableImage.Chances.Count == 0)
                    continue;

                if (spawnableImage.Chances.All(p => p.Key.StartsWith("example")))
                    continue;
                
                var imageToSpawn = spawnableImage.Chances.GetRandomWeighted(p => p.Value);

                if (!string.IsNullOrEmpty(imageToSpawn.Key) && !string.Equals("none", imageToSpawn.Key, StringComparison.InvariantCultureIgnoreCase) 
                                                            && ImageLoader.TryGet(imageToSpawn.Key, out var image))
                {
                    var toy = image.SpawnImage(spawnableImage.Position.Vector, spawnableImage.Rotation.Quaternion);

                    if (toy?.Base != null)
                    {
                        spawnedList.Add(new(spawnableImage, toy));

                        Spawned?.InvokeSafe(spawnablePair.Key, spawnableImage, toy);
                    }
                }
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