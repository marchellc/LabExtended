using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Attributes;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.API.Images;

/// <summary>
/// Used to load custom-format image files.
/// </summary>
public static class ImageLoader
{
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
    
    internal static ImageFile ReadFile(byte[] data, string name)
    {
        var image = new ImageFile() { Name = name };
        
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
                        
                        pixel.Color = new Color(
                            reader.ReadByte(), 
                            reader.ReadByte(), 
                            reader.ReadByte(), 
                            reader.ReadByte());
                        
                        pixels.Add(pixel);
                    }
                    
                    frame.Pixels.Add(pixels);
                }
                
                image.Frames.Add(frame);
            }

            image.ConvertFormats();
        }

        return image;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        if (!System.IO.Directory.Exists(Directory))
            System.IO.Directory.CreateDirectory(Directory);

        foreach (var filePath in System.IO.Directory.GetFiles(Directory))
        {
            try
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                var data = File.ReadAllBytes(filePath);
                
                LoadedImages.Add(name, ReadFile(data, name));
            }
            catch (Exception ex)
            {
                ApiLog.Error("Image Loader", $"Failed while trying to load image file &3{Path.GetFileName(filePath)}&r:\n{ex}");
            }
        }
    }
}