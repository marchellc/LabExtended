using NorthwoodLib.Pools;
using UnityEngine;

namespace LabExtended.API.Images.Conversion;

/// <summary>
/// Converts an image to be used in text toys.
/// </summary>
public static class ToyStringImageConvertor
{
    /// <summary>
    /// Gets or sets the character used to represent a pixel.
    /// </summary>
    public static char Character { get; set; } = '█';
    
    /// <summary>
    /// Gets or sets the height of the character used to represent a pixel (in percentage).
    /// </summary>
    public static int CharacterSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the height of the character used to represent a pixel (in percentage).
    /// </summary>
    public static int CharacterHeight { get; set; } = 75;
    
    /// <summary>
    /// Assigns text toy data to each image frame.
    /// </summary>
    /// <param name="file">The target image file.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ConvertImage(ImageFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        var builder = StringBuilderPool.Shared.Rent();

        Color? last = null;
        
        file.toyDisplaySize = new(file.Height * 10, file.Width * 10);
        
        for (var index = 0; index < file.Frames.Count; index++)
        {
            var frame = file.Frames[index];
            
            for (var x = 0; x < frame.Pixels.Count; x++)
            {
                builder.Clear();

                builder.Append("<size=");
                builder.Append(CharacterSize);
                builder.Append("%><line-height=");
                builder.Append(CharacterHeight);
                builder.Append("%>");

                var pixels = frame.Pixels[x];

                for (var y = 0; y < pixels.Count; y++)
                {
                    var pixel = pixels[y];

                    if (!last.HasValue || last.Value != pixel.Color)
                    {
                        builder.Append("<color=");
                        builder.Append(pixel.Color.ToHex());
                        builder.Append(">");
                        
                        last = pixel.Color;
                    }

                    builder.Append(Character);
                }

                builder.Append("</color>");

                frame.toyFrameData.Add(builder.ToString());
            }
            
            builder.Clear();

            for (var i = 0; i < file.Height; i++)
            {
                builder.Append("{");
                builder.Append(i);
                builder.Append("}");
                builder.Append("\\n");
            }

            frame.toyFrameFormat = builder.ToString();
        }
        
        StringBuilderPool.Shared.Return(builder);
    }
}