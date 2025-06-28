using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Net;
using System.Numerics;
using CommonLib;
using CommonLib.Extensions;
using CommonLib.Utilities.Console;

namespace LabExtended.ImageConvertor;

/// <summary>
/// Converts images.
/// </summary>
public static class ImageConversion
{
    /// <summary>
    /// Gets the input directory path.
    /// </summary>
    public static string InputDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Input");
    
    /// <summary>
    /// Gets the output directory path.
    /// </summary>
    public static string OutputDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Output");
    
    /// <summary>
    /// Starts a new conversion.
    /// </summary>
    public static void Run()
    {
        if (!Directory.Exists(InputDirectory))
            Directory.CreateDirectory(InputDirectory);
        
        if (!Directory.Exists(OutputDirectory))
            Directory.CreateDirectory(OutputDirectory);
        
        CommonLog.Info("Conversion", "Loading input files ..");

        var urlList = ConsoleArgs.GetValueOrDefault("url", string.Empty);

        if (urlList != string.Empty)
        {
            var urls = urlList.Split(',');
            var client = new WebClient();
            
            CommonLog.Info("Conversion", $"Downloading '{urls.Length}' image(s).");
            
            for (var i = 0; i < urls.Length; i++)
            {
                var url = new Uri(urls[i]);
                
                client.DownloadFile(url, Path.Combine(InputDirectory, Path.GetFileName(url.LocalPath)));
            }
            
            CommonLog.Info("Conversion", "Finished downloading.");
        }

        foreach (var filePath in Directory.GetFiles(InputDirectory))
        {
            CommonLog.Info("Conversion", $"Starting conversion of file '{Path.GetFileName(filePath)}'");

            Task.Run(() =>
                Convert(filePath, Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(filePath))));
        }
    }

    private static void Convert(string inputPath, string outputPath)
    {
        try
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            
            var name = Path.GetFileName(inputPath);
            
            var sourceImage = Image.FromFile(inputPath);
            var sourceFrames = new List<Bitmap>();
            
            var imageDelay = 0f;
            
            var resolutionHeight = ConsoleArgs.GetValueOrDefault("height", int.Parse, sourceImage.Height);
            var resolutionWidth = ConsoleArgs.GetValueOrDefault("width", int.Parse, sourceImage.Width);

            var toyStringSize = ConsoleArgs.GetValueOrDefault("textToySize", int.Parse, 33);
            var toyStringHeight = ConsoleArgs.GetValueOrDefault("textToyHeight", int.Parse, 75);
            
            var toyScale = ConsoleArgs.GetValueOrDefault("textToyScale", ParseVector3, Vector3.One);
            var toyDisplay = ConsoleArgs.GetValueOrDefault("textToyDisplay", ParseVector2, Vector2.Zero);
            
            if (sourceImage.IsAnimated())
            {
                CommonLog.Debug("Conversion", $"Converting animated image '{name}' ({resolutionHeight}x{resolutionWidth})");
                
                var count = sourceImage.GetFrameCount(FrameDimension.Time);
                
                CommonLog.Debug("Conversion", $"Frames: {count}");
                
                if (sourceFrames.Capacity < count)
                    sourceFrames.Capacity = count;

                imageDelay = sourceImage.GetFrameDelay();
                
                CommonLog.Debug("Conversion", $"Delay: {imageDelay} ms ({Math.Ceiling(1000 / imageDelay)} FPS)");
                
                for (var i = 0; i < count; i++)
                {
                    sourceImage.SelectActiveFrame(FrameDimension.Time, i);
                    sourceFrames.Add(new(sourceImage, new Size(resolutionWidth, resolutionHeight)));
                }
            }
            else
            {
                CommonLog.Debug("Conversion", $"Converting static image '{name}' ({resolutionHeight}x{resolutionWidth})");
                
                sourceFrames.Add(new(sourceImage, new Size(resolutionWidth, resolutionHeight)));
            }

            if (toyDisplay == Vector2.Zero)
            {
                toyDisplay.X = resolutionWidth;
                toyDisplay.Y = resolutionHeight;
            }
            
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Path.GetExtension(inputPath));

                writer.Write(imageDelay);

                writer.Write(File.GetCreationTimeUtc(inputPath).Ticks);

                writer.Write(resolutionHeight);
                writer.Write(resolutionWidth);

                writer.Write(sourceFrames.Count);

                foreach (var frame in sourceFrames)
                {
                    for (var y = 0; y < resolutionHeight; y++)
                    {
                        for (var x = 0; x < resolutionWidth; x++)
                        {
                            var pixel = frame.GetPixel(x, y);

                            writer.Write(pixel.R);
                            writer.Write(pixel.G);
                            writer.Write(pixel.B);
                            writer.Write(pixel.A);
                        }
                    }
                }

                writer.Write(toyStringSize);
                writer.Write(toyStringHeight);

                writer.Write(toyScale.X);
                writer.Write(toyScale.Y);
                writer.Write(toyScale.Z);
                
                writer.Write(toyDisplay.X);
                writer.Write(toyDisplay.Y);

                File.WriteAllBytes(outputPath, stream.ToArray());
            }

            CommonLog.Info("Conversion", $"Finished converting '{Path.GetFileName(inputPath)}'");
        }
        catch (Exception ex)
        {
            CommonLog.Error("Conversion", $"An error occured while attempting to convert file '{Path.GetFileName(inputPath)}':\n{ex}");
        }
    }
    
    private static Vector2 ParseVector2(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Vector2.One;

        if (!str.TrySplit(',', true, null, out var parts))
        {
            CommonLog.Warn("Conversion", $"Could not parse input vector string '{str}'");
            return Vector2.One;
        }

        if (parts.Length > 0)
        {
            if (!float.TryParse(parts[0], NumberFormatInfo.InvariantInfo, out var x))
            {
                CommonLog.Warn("Conversion", $"Could not parse vector X axis ('{parts[0]}')");
                return Vector2.One;
            }
            
            if (parts.Length > 1)
            {
                if (!float.TryParse(parts[1], NumberFormatInfo.InvariantInfo, out var y))
                {
                    CommonLog.Warn("Conversion", $"Could not parse vector Y axis ('{parts[1]}')");
                    return Vector2.One;
                }
                
                return new Vector2(x, y);
            }

            return new Vector2(x, 1f);
        }
        
        return Vector2.One;
    }

    private static Vector3 ParseVector3(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Vector3.One;

        if (!str.TrySplit(',', true, null, out var parts))
        {
            CommonLog.Warn("Conversion", $"Could not parse input vector string '{str}'");
            return Vector3.One;
        }

        if (parts.Length > 0)
        {
            if (!float.TryParse(parts[0], NumberFormatInfo.InvariantInfo, out var x))
            {
                CommonLog.Warn("Conversion", $"Could not parse vector X axis ('{parts[0]}')");
                return Vector3.One;
            }
            
            if (parts.Length > 1)
            {
                if (!float.TryParse(parts[1], NumberFormatInfo.InvariantInfo, out var y))
                {
                    CommonLog.Warn("Conversion", $"Could not parse vector Y axis ('{parts[1]}')");
                    return Vector3.One;
                }

                if (parts.Length > 2)
                {
                    if (!float.TryParse(parts[2], NumberFormatInfo.InvariantInfo, out var z))
                    {
                        CommonLog.Warn("Conversion", $"Could not parse vector Z axis ('{parts[2]}')");
                        return Vector3.One;
                    }
                    
                    return new Vector3(x, y, z);
                }
                
                return new Vector3(x, y, 1f);
            }

            return new Vector3(x, 1f, 1f);
        }
        
        return Vector3.One;
    }
}