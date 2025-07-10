using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Attributes;
using LabExtended.Utilities.Update;

using NVorbis;

using VoiceChat;

namespace LabExtended.API.Audio;

/// <summary>
/// Manages audio spawning.
/// </summary>
public static class AudioManager
{
    private static FileSystemWatcher watcher;

    /// <summary>
    /// Gets the offset of reserved speaker IDs. Funny things could happen if a plugin ignores this.
    /// </summary>
    public const byte ReservedIdOffset = 10;
    
    /// <summary>
    /// Gets called once per frame.
    /// </summary>
    public static event Action? Update;

    /// <summary>
    /// Gets called when an audio clip's file is added.
    /// </summary>
    public static event Action<KeyValuePair<string, byte[]>>? ClipAdded; 
    
    /// <summary>
    /// Gets called when an audio clip's file is removed.
    /// </summary>
    public static event Action<KeyValuePair<string, byte[]>>? ClipRemoved;

    /// <summary>
    /// Gets called when an audio clip's file is modified.
    /// </summary>
    public static event Action<KeyValuePair<string, byte[]>>? ClipUpdated; 
    
    /// <summary>
    /// Gets the path to the directory with audio clip files.
    /// </summary>
    public static string ClipDirectory { get; private set; }

    /// <summary>
    /// Gets a list of all created audio handlers.
    /// </summary>
    public static Dictionary<string, AudioHandler> Handlers { get; } = new();

    /// <summary>
    /// Gets a list of all loaded audio clips.
    /// </summary>
    public static List<KeyValuePair<string, byte[]>> Clips { get; } = new();

    /// <summary>
    /// Gets a loaded audio clip.
    /// </summary>
    /// <param name="name">The name of the clip's file.</param>
    /// <returns>The loaded audio clip.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static KeyValuePair<string, byte[]> GetClip(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (!Clips.TryGetFirst(x => x.Key == name, out var clip))
            throw new KeyNotFoundException($"Audio clip {name} not found");

        return clip;
    }

    /// <summary>
    /// Gets an existing audio handler.
    /// </summary>
    /// <param name="name">The name of the audio handler.</param>
    /// <returns>The audio handler instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static AudioHandler GetHandler(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (!Handlers.TryGetValue(name, out var handler))
            throw new KeyNotFoundException($"Audio handler {name} not found");
        
        return handler;
    }

    /// <summary>
    /// Gets an existing handler or adds a new one.
    /// </summary>
    /// <param name="name">The name of the audio handler.</param>
    /// <param name="handlerSetup">The delegate used to setup the audio handler.</param>
    /// <returns>The found or created audio handler.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static AudioHandler GetOrAddHandler(string name, Action<AudioHandler>? handlerSetup = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (Handlers.TryGetValue(name, out var handler))
            return handler;

        handler = new();
        handler.Name = name;
        
        handlerSetup?.InvokeSafe(handler);
        
        Handlers.Add(name, handler);
        return handler;
    }

    /// <summary>
    /// Attempts to retrieve a loaded audio clip.
    /// </summary>
    /// <param name="name">The name of the audio clip.</param>
    /// <param name="clip">The retrieved audio clip.</param>
    /// <returns>true if the audio clip was found</returns>
    public static bool TryGetClip(string name, out KeyValuePair<string, byte[]> clip)
    {
        clip = default;

        if (string.IsNullOrEmpty(name))
            return false;
        
        return Clips.TryGetFirst(x => x.Key == name, out clip);
    }

    /// <summary>
    /// Attempts to retrieve an existing audio handler.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="handler"></param>
    /// <returns>true if the audio handler was found</returns>
    public static bool TryGetHandler(string name, out AudioHandler handler)
    {
        handler = null;

        if (string.IsNullOrEmpty(name))
            return false;

        return Handlers.TryGetValue(name, out handler);
    }

    /// <summary>
    /// Reloads all audio clips.
    /// </summary>
    public static void LoadClips()
    {
        if (Clips.Count > 0 && ClipRemoved != null)
        {
            foreach (var clip in Clips)
            {
                ClipRemoved?.InvokeSafe(clip);
            }
        }

        Clips.Clear();

        foreach (var file in Directory.GetFiles(ClipDirectory))
        {
            if (TryRead(file, out var clip))
            {
                Clips.Add(clip);
                    
                ClipAdded?.InvokeSafe(clip);
            }
        }
    }

    private static bool TryRead(string filePath, out KeyValuePair<string, byte[]> clip)
    {
        try
        {
            clip = default;
            
            var name = Path.GetFileNameWithoutExtension(filePath);
            var data = File.ReadAllBytes(filePath);

            using (var stream = new MemoryStream(data))
            using (var reader = new VorbisReader(stream))
            {
                if (reader.TotalSamples < 1)
                {
                    ApiLog.Warn("Audio Manager", $"Could not load audio clip &3{name}&r: no audio samples");
                    return false;
                }

                if (reader.Channels != VoiceChatSettings.Channels)
                {
                    ApiLog.Warn("Audio Manager", $"Could not load audio clip &3{name}&r: only mono audio is supported (channels: {reader.Channels})");
                    return false;
                }

                if (reader.SampleRate != VoiceChatSettings.SampleRate)
                {
                    ApiLog.Warn("Audio Manager", $"Could not load audio clip &3{name}&r: only a sampling rate of {VoiceChatSettings.SampleRate} Hz is supported (rate: {reader.SampleRate} Hz)");
                    return false;
                }

                clip = new KeyValuePair<string, byte[]>(name, data);
                return true;
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Audio Manager", $"Error while loading audio clip &3{Path.GetFileNameWithoutExtension(filePath)}&r:\n{ex}");
        }

        clip = default;
        return false;
    }
    
    private static void OnUpdate()
    {
        Update?.InvokeSafe();
    }

    private static void OnFileDeleted(object _, FileSystemEventArgs ev)
    {
        var name = Path.GetFileNameWithoutExtension(ev.FullPath);

        if (!Clips.TryGetFirst(x => x.Key == name, out var clip))
            return;
        
        Clips.Remove(clip);
        
        ClipRemoved?.InvokeSafe(clip);
    }

    private static void OnFileAdded(object _, FileSystemEventArgs ev)
    {
        var name = Path.GetFileNameWithoutExtension(ev.FullPath);

        if (!TryRead(ev.FullPath, out var newClip))
            return;
        
        if (Clips.TryGetFirst(x => x.Key == name, out var clip))
        {
            Clips.Remove(clip);
            Clips.Add(newClip);
            
            ClipUpdated?.InvokeSafe(newClip);
        }
        else
        {
            Clips.Add(newClip);
            ClipAdded?.InvokeSafe(newClip);
        }
    }

    private static void OnFileModified(object _, FileSystemEventArgs ev)
        => OnFileAdded(_, ev);
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ClipDirectory = Path.Combine(ApiLoader.DirectoryPath, "Audio");

        if (!Directory.Exists(ClipDirectory))
            Directory.CreateDirectory(ClipDirectory);
        
        LoadClips();

        watcher = new(ClipDirectory);
        watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName |
                               NotifyFilters.LastWrite;
        
        watcher.Deleted += OnFileDeleted;
        watcher.Created += OnFileAdded;
        watcher.Changed += OnFileModified;

        watcher.EnableRaisingEvents = true;
        
        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }
}