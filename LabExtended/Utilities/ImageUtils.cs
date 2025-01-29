using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Utilities;

public static class ImageUtils
{
    public static string ToHex(this Color c) 
        => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    
    public static Bitmap[] ExtractFrames(this Image image, int? horizontalResolution = null, int? verticalResolution = null)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));

        var height = horizontalResolution.HasValue ? horizontalResolution.Value : image.Height;
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

    public static UnityEngine.Color?[,] ToPrimitiveFrame(this Bitmap frame, ref UnityEngine.Color?[,] previousFrame)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));
        
        var current = new UnityEngine.Color?[frame.Width, frame.Height];
        
        for (int x = 0; x < frame.Height; x++)
        {
            for (int y = 0; y < frame.Width; y++)
            {
                var pixel = frame.GetPixel(y, x);
                var pixelUnity = new UnityEngine.Color(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, pixel == Color.Transparent ? 0 : 1);

                if (previousFrame != null && previousFrame[y, x] == pixelUnity)
                {
                    current[x, y] = null;
                    continue;
                }
                    
                current[x, y] = pixelUnity;
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
        var last = default(UnityEngine.Color?[,]);

        for (int i = 0; i < frames.Length; i++)
            list.Add(frames[i].ToPrimitiveFrame(ref last));

        return list;
    }

    public static string ToHintFrame(this Bitmap frame, ref Color lastColor, int size = 33, int height = 75, char pixelCharacter = '█', StringBuilder builder = null)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));

        var ownsBuilder = builder is null;

        if (builder is null)
            builder = StringBuilderPool.Shared.Rent();

        for (int x = 0; x < frame.Height; x++)
        {
            builder.Append("<size=");
            builder.Append(size);
            builder.Append("%><line-height=");
            builder.Append(height);
            builder.Append("%>");

            for (int y = 0; y < frame.Width; y++)
            {
                var pixel = frame.GetPixel(y, x);

                if (pixel != lastColor)
                {
                    builder.Append("<color=");
                    builder.Append(ToHex(pixel));
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