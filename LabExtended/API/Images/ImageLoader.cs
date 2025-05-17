using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Attributes;

using NorthwoodLib.Pools;

using System.Drawing;

using LabExtended.API.Images.Conversion;
using LabExtended.Core.Pooling.Pools;

namespace LabExtended.API.Images;

/// <summary>
/// Used to load custom-format image files.
/// </summary>
public static class ImageLoader
{
    private static FileSystemWatcher watcher;
    
    /// <summary>
    /// Gets a list of all loaded images.
    /// </summary>
    public static Dictionary<string, ImageFile> LoadedImages { get; } = new();

    /// <summary>
    /// Gets the image directory.
    /// </summary>
    public static string Directory { get; } = Path.Combine(ApiLoader.DirectoryPath, "Images");

    /// <summary>
    /// Gets an image by it's file name.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The loaded image file.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static ImageFile Get(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (!LoadedImages.TryGetValue(name, out var file))
            throw new KeyNotFoundException($"No images named {name} were found");
        
        return file;
    }
    
    /// <summary>
    /// Attempts to get an image by it's file name.
    /// </summary>
    /// <param name="name">The name of the image file.</param>
    /// <param name="file">The resolved image file.</param>
    /// <returns>true if the image was found</returns>
    public static bool TryGet(string name, out ImageFile file)
        => LoadedImages.TryGetValue(name, out file);
    
    internal static ImageFile ReadFile(byte[] data)
    {
        var image = new ImageFile();
        
        using (var stream = new MemoryStream(data))
        using (var reader = new BinaryReader(stream))
        {
            image.Extension = reader.ReadString();
            image.FrameDuration = reader.ReadSingle();
            
            image.CreatedAt = new DateTime(reader.ReadInt64()).ToLocalTime();
            
            image.Height = reader.ReadInt32();
            image.Width = reader.ReadInt32();
            
            var frameCount = reader.ReadInt32();

            for (var i = 0; i < frameCount; i++)
            {
                var frame = new ImageFrame()
                {
                    Index = i,
                    File = image
                };

                if (i - 1 > 0)
                {
                    frame.PreviousFrame = image.Frames[i - 1];
                    frame.PreviousFrame.NextFrame = frame;
                }

                for (var y = 0; y < image.Height; y++)
                {
                    var pixels = ListPool<ImagePixel>.Shared.Rent(image.Width);
                    
                    for (var x = 0; x < image.Width; x++)
                    {
                        var pixel = new ImagePixel()
                        {
                            X = x,
                            Y = y,

                            Frame = frame
                        };

                        if (x - 1 > 0)
                        {
                            pixel.PreviousPixel = pixels[x - 1];
                            pixel.PreviousPixel.NextPixel = pixel;
                        }

                        var red = reader.ReadByte();
                        var green = reader.ReadByte();
                        var blue = reader.ReadByte();
                        var alpha = reader.ReadByte();
                        
                        pixel.Color = Color.FromArgb(alpha, red, green, blue);
                        
                        pixels.Add(pixel);
                    }
                    
                    frame.Pixels.Add(pixels);
                }
                
                image.Frames.Add(frame);
            }
            
            ToyStringImageConvertor.ReadImage(image, reader);
        }
        
        image.ConvertFormats();
        return image;
    }

    private static void OnCreated(object _, FileSystemEventArgs ev)
    {
        if (!string.IsNullOrEmpty(Path.GetExtension(ev.FullPath)))
            return;

        var name = Path.GetFileNameWithoutExtension(ev.FullPath);
        
        if (LoadedImages.TryGetValue(name, out var curImage))
            curImage.Dispose();

        LoadedImages.Remove(name);
        
        Task.Run(() =>
        {
            try
            {

                var data = File.ReadAllBytes(ev.FullPath);
                var image = ReadFile(data);

                image.Name = name;
                image.Path = ev.FullPath;

#if IMAGE_LOADER_DEBUG
                    ToyStringImageConvertor.DebugImage(image, Path.Combine(Directory, image.Name + "_ToyStringImageConvertorDebug.txt"));
#endif

                LoadedImages.Add(name, image);

                ApiLog.Debug("Image Loader", $"Loaded image &6{image.Name}&r");
            }
            catch (Exception ex)
            {
                ApiLog.Error("Image Loader",
                    $"Failed while trying to load image file &3{Path.GetFileName(ev.FullPath)}&r:\n{ex}");
            }
        });
    }

    private static void OnDeleted(object _, FileSystemEventArgs ev)
    {
        var images = DictionaryPool<string, ImageFile>.Shared.Rent(LoadedImages);

        try
        {
            foreach (var image in images)
            {
                if (image.Value.Path == ev.FullPath)
                {
                    image.Value.Dispose();
                    
                    ApiLog.Debug("Image Loader", $"Removed image &6{image.Value.Name}&r");
                    
                    LoadedImages.Remove(image.Key);
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Image Loader",
                $"Failed while trying to un-load image file &3{Path.GetFileName(ev.FullPath)}&r:\n{ex}");
        }
        
        DictionaryPool<string, ImageFile>.Shared.Return(images);
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        Task.Run(() =>
        {
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);

            foreach (var filePath in System.IO.Directory.GetFiles(Directory))
            {
                try
                {
                    if (!string.IsNullOrEmpty(Path.GetExtension(filePath)))
                        continue;
                    
                    var data = File.ReadAllBytes(filePath);
                    var image = ReadFile(data);
                    
                    image.Name = Path.GetFileNameWithoutExtension(filePath);
                    image.Path = filePath;
                    
#if IMAGE_LOADER_DEBUG
                    ToyStringImageConvertor.DebugImage(image, Path.Combine(Directory, image.Name + "_ToyStringImageConvertorDebug.txt"));
#endif
                    
                    LoadedImages.Add(image.Name, image);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Image Loader",
                        $"Failed while trying to load image file &3{Path.GetFileName(filePath)}&r:\n{ex}");
                }
            }
        }).ContinueWithOnMain(_ =>
        {
            watcher = new(Directory);
            
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;

            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName |
                                   NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            
            watcher.EnableRaisingEvents = true;
            
            ApiLog.Debug("Image Loader", $"Loaded &6{LoadedImages.Count}&r image(s).");
        });
    }
}