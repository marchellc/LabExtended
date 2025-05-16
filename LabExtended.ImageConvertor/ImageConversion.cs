using System.Drawing;
using System.Drawing.Imaging;

using System.Net;

using CommonLib;
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
            
            var resolutionHeight = ConsoleArgs.GetValueOrDefault<int>("height", int.Parse, sourceImage.Height);
            var resolutionWidth = ConsoleArgs.GetValueOrDefault<int>("width", int.Parse, sourceImage.Width);
            
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
                
                File.WriteAllBytes(outputPath, stream.ToArray());
            }
            
            CommonLog.Info("Conversion", $"Finished converting '{Path.GetFileName(inputPath)}'");
        }
        catch (Exception ex)
        {
            CommonLog.Error("Conversion", $"An error occured while attempting to convert file '{Path.GetFileName(inputPath)}':\n{ex}");
        }
    }
}