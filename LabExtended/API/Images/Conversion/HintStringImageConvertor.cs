using NorthwoodLib.Pools;

using System.Drawing;

using LabExtended.Utilities;

namespace LabExtended.API.Images.Conversion;

/// <summary>
/// Converts an image to be used in hint overlays.
/// </summary>
public static class HintStringImageConvertor
{
    /// <summary>
    /// Gets or sets the character used to represent a pixel.
    /// </summary>
    public static char Character { get; set; } = '█';
    
    /// <summary>
    /// Gets or sets the height of the character used to represent a pixel (in percentage).
    /// </summary>
    public static int CharacterSize { get; set; } = 33;

    /// <summary>
    /// Gets or sets the height of the character used to represent a pixel (in percentage).
    /// </summary>
    public static int CharacterHeight { get; set; } = 75;
    
    /// <summary>
    /// Assigns hint data to each image frame.
    /// </summary>
    /// <param name="file">The target image file.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ConvertImage(ImageFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));
        
        var builder = StringBuilderPool.Shared.Rent();

        Color? last = null;
        
        file.Frames.ForEach(frame =>
        {
            builder.Clear();
            
            builder.Append("<size=");
            builder.Append(CharacterSize);
            builder.Append("%><line-height=");
            builder.Append(CharacterHeight);
            builder.Append("%>");

            for (var x = 0; x < frame.Pixels.Count; x++)
            {
                var pixels = frame.Pixels[x];

                for (var y = 0; y < pixels.Count; y++)
                {
                    var pixel = pixels[y];

                    if (!last.HasValue || last.Value != pixel.Color)
                    {
                        builder.Append("<color=");
                        builder.Append(pixel.Color.ToShortHex());
                        builder.Append(">");

                        last = pixel.Color;
                    }

                    builder.Append(Character);
                }

                builder.AppendLine();
            }

            builder.Append("</color>");

            frame.hintFrameData = builder.ToString();
        });
        
        StringBuilderPool.Shared.Return(builder);
    }
}