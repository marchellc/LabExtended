using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using LabExtended.Extensions;

using NorthwoodLib.Pools;
using UnityEngine;
using Color = System.Drawing.Color;

namespace LabExtended.Utilities;

public static class ImageUtils
{
    public static string FullHexToShortHex(this string hex) 
    {
        int i = 0;
        
        if (hex[0] == '#') 
            i = 1;

        if (hex.Length -i != 6) 
            throw new Exception("Invalid hex format. Expected #RRGGBB.");

        int r = int.Parse(hex.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(i + 2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(i + 4, 2), System.Globalization.NumberStyles.HexNumber);

        int rShort = Mathf.RoundToInt(r / 17f);
        int gShort = Mathf.RoundToInt(g / 17f);
        int bShort = Mathf.RoundToInt(b / 17f);

        return $"#{rShort:x}{gShort:x}{bShort:x}";
    }
    
    public static string ToShortHex(this Color color) 
    {
        int rShort = Mathf.RoundToInt(color.R / 17f);
        int gShort = Mathf.RoundToInt(color.G / 17f);
        int bShort = Mathf.RoundToInt(color.B / 17f);

        return $"#{rShort:x}{gShort:x}{bShort:x}";
    }

    public static string ToHex(this Color c) 
        => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    
    public static Bitmap[] ExtractFrames(this Image image, int? horizontalResolution = null, int? verticalResolution = null)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));

        var height = verticalResolution.HasValue ? verticalResolution.Value : image.Height;
        var width = horizontalResolution.HasValue ? horizontalResolution.Value : image.Width;
        
        if (!image.FrameDimensionsList.Contains(FrameDimension.Time.Guid))
            return new Bitmap[] { new Bitmap(image, new Size(width, height)) };

        var count = image.GetFrameCount(FrameDimension.Time);
        
        if (count <= 0)
            throw new Exception("Provided image doesn't contain any frames");
        
        var frames = ListPool<Bitmap>.Shared.Rent(count);

        for (int i = 0; i < count; i++)
        {
            image.SelectActiveFrame(FrameDimension.Time, i);
            frames.Add(new Bitmap(image, new Size(width, height)));
        }

        return ListPool<Bitmap>.Shared.ToArrayReturn(frames);
    }

    public static UnityEngine.Color?[,] ToPrimitiveFrame(this Bitmap frame, UnityEngine.Color?[,] previousFrame = null)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));
        
        var current = new UnityEngine.Color?[frame.Height, frame.Width];
        
        for (int i = 0; i < frame.Height; i++)
        {
            for (int y = 0; y < frame.Width; y++)
            {
                var pixel = frame.GetPixel(y, i);
                var pixelUnity = new UnityEngine.Color(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, pixel == System.Drawing.Color.Transparent ? 0 : 1);

                if (previousFrame != null && previousFrame[i, y] == pixelUnity)
                {
                    current[i, y] = null;
                    continue;
                }
                
                current[i, y] = pixelUnity;
            }
        }
        
        return current;
    }
    
    public static List<UnityEngine.Color?[,]> ToPrimitiveFrames(this Bitmap[] frames)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Length == 0)
            throw new Exception("Provided image doesn't contain any frames");

        var list = new List<UnityEngine.Color?[,]>(frames.Length);
        
        UnityEngine.Color?[,]? lastFrame = null;

        for (int i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            var conv = frame.ToPrimitiveFrame(lastFrame);

            lastFrame = conv;
            list.Add(conv);
        }

        return list;
    }

    public static string ToHintFrame(this Bitmap frame, ref Color lastColor, int size = 33, int height = 75, char pixelCharacter = '█', StringBuilder builder = null)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));

        var ownsBuilder = builder is null;

        if (builder is null)
            builder = StringBuilderPool.Shared.Rent();

        builder.Append("<size=");
        builder.Append(size);
        builder.Append("%><line-height=");
        builder.Append(height);
        builder.Append("%>");
        
        for (int x = 0; x < frame.Height; x++)
        {
            for (int y = 0; y < frame.Width; y++)
            {
                var pixel = frame.GetPixel(y, x);

                if (pixel != lastColor)
                {
                    builder.Append("<color=");
                    builder.Append(ToShortHex(pixel));
                    builder.Append(">");

                    lastColor = pixel;
                }

                builder.Append(pixelCharacter);
            }

            builder.AppendLine();
        }

        builder.Append("</color>");

        if (ownsBuilder)
            return StringBuilderPool.Shared.ToStringReturn(builder);

        return null;
    }

    public static string[] ToHintFrames(this Bitmap[] frames, int size = 33, int height = 75, char pixelCharacter = '█')
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Length == 0)
            throw new Exception("Provided image doesn't contain any frames");
        
        var list = ListPool<string>.Shared.Rent(frames.Length);
        var builder = StringBuilderPool.Shared.Rent();
        var lastColor = Color.White;

        for (int i = 0; i < frames.Length; i++)
        {
            builder.Clear();
            
            frames[i].ToHintFrame(ref lastColor, size, height, pixelCharacter, builder);
            
            list.Add(builder.ToString());
        }
        
        StringBuilderPool.Shared.Return(builder);
        return ListPool<string>.Shared.ToArrayReturn(list);
    }
}