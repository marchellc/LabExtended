using System.Drawing;

using LabExtended.Extensions;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.API.Toys.Text;

/// <summary>
/// Utilities targeting text image toys.
/// </summary>
public static class TextImageToyUtils
{
    /// <summary>
    /// Creates a display format for a specific line count.
    /// </summary>
    /// <param name="count">The line count.</param>
    /// <returns>The created format.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string CreateDisplayFormat(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var builder = StringBuilderPool.Shared.Rent();

        for (var i = 0; i < count; i++)
        {
            builder.Append("{");
            builder.Append(i);
            builder.Append("}");
            builder.Append("\n");
        }

        return StringBuilderPool.Shared.ToStringReturn(builder);
    }

    /// <summary>
    /// Gets the required display size for an image.
    /// </summary>
    /// <param name="image">The target image.</param>
    /// <returns>The required display size vector.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static Vector2 GetDisplaySize(Image image)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image));
        
        return new(image.Width, image.Height);
    }

    /// <summary>
    /// Generates a list of lines from an image frame.
    /// </summary>
    /// <param name="frame">The bitmap frame.</param>
    /// <param name="displaySize">The required display size.</param>
    /// <param name="size">The character size (percentage)..</param>
    /// <param name="lineHeight">The character height (percentage).</param>
    /// <param name="pixelCharacter">The character used to display pixels.</param>
    /// <returns>The generated string list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<string> CreateImage(Bitmap frame, out Vector2 displaySize, int size = 33,
        int lineHeight = 75, char pixelCharacter = '█')
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));

        var highestX = 0f;
        var highestY = 0f;

        var list = ListPool<string>.Shared.Rent(1);
        var builder = StringBuilderPool.Shared.Rent();

        System.Drawing.Color? lastColor = null;

        if (frame.Height > highestY)
            highestY = frame.Height;

        if (frame.Width > highestX)
            highestX = frame.Width;

        for (var height = 0; height < frame.Width; height++)
        {
            builder.Clear();
            
            builder.Append("<size=");
            builder.Append(size);
            builder.Append("%><line-height=");
            builder.Append(lineHeight);
            builder.Append("%>");
            
            for (var width = 0; width < frame.Width; width++)
            {
                var pixel = frame.GetPixel(width, height);
                var hex = pixel.ToShortHex();

                if (lastColor.GetValueOrDefault() != pixel)
                {
                    builder.Append("<color=");
                    builder.Append(hex);
                    builder.Append(">");

                    lastColor = pixel;
                }

                builder.Append(pixelCharacter);
            }

            builder.Append("</color>");
            
            list.Add(builder.ToString());
        }

        StringBuilderPool.Shared.Return(builder);

        displaySize = new(highestX, highestY);
        return list;
    }

    /// <summary>
    /// Generates a list of lines from an image frame collection.
    /// </summary>
    /// <param name="frames">The bitmap frames.</param>
    /// <param name="displaySize">The required display size.</param>
    /// <param name="size">The character size (percentage)..</param>
    /// <param name="lineHeight">The character height (percentage).</param>
    /// <param name="pixelCharacter">The character used to display pixels.</param>
    /// <returns>The generated string list.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<List<string>> CreateImage(IEnumerable<Bitmap> frames, out Vector2 displaySize, int size = 33, 
        int lineHeight = 75, char pixelCharacter = '█')
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));

        var highestX = 0f;
        var highestY = 0f;
        
        var count = frames is IList<Bitmap> bitmapList 
            ? bitmapList.Count 
            : (frames is Bitmap[] array 
                ? array.Length 
                : 0);
        
        var list = ListPool<List<string>>.Shared.Rent(count);
        
        var builder = StringBuilderPool.Shared.Rent();
        var frameBuilder = StringBuilderPool.Shared.Rent();

        System.Drawing.Color? lastColor = null;
        
        builder.Append("<size=");
        builder.Append(size);
        builder.Append("%><line-height=");
        builder.Append(lineHeight);
        builder.Append("%>");
        
        frames.ForEach(frame =>
        {
            var lines = ListPool<string>.Shared.Rent();

            if (frame.Height > highestY)
                highestY = frame.Height;
            
            if (frame.Width > highestX)
                highestX = frame.Width;

            for (var height = 0; height < frame.Width; height++)
            {
                frameBuilder.Clear();
                frameBuilder.Append(builder);
                
                for (var width = 0; width < frame.Width; width++)
                {
                    var pixel = frame.GetPixel(width, height);
                    var hex = pixel.ToShortHex();

                    if (lastColor.GetValueOrDefault() != pixel)
                    {
                        frameBuilder.Append("<color=");
                        frameBuilder.Append(hex);
                        frameBuilder.Append(">");
                        
                        lastColor = pixel;
                    }

                    frameBuilder.Append(pixelCharacter);
                }

                frameBuilder.Append("</color>");
                
                lines.Add(frameBuilder.ToString());
            }
            
            list.Add(lines);
        });
        
        StringBuilderPool.Shared.Return(builder);
        StringBuilderPool.Shared.Return(frameBuilder);
        
        displaySize = new(highestX, highestY);
        return list;
    }
}