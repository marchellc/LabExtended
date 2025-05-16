using UnityEngine;

namespace LabExtended.API.Images;

/// <summary>
/// Represents a pixel of a custom-format image.
/// </summary>
public class ImagePixel
{
    /// <summary>
    /// Gets the pixel's frame.
    /// </summary>
    public ImageFrame Frame { get; internal set; }
    
    /// <summary>
    /// Gets the next pixel.
    /// </summary>
    public ImagePixel? NextPixel { get; internal set; }
    
    /// <summary>
    /// Gets the previous pixel.
    /// </summary>
    public ImagePixel? PreviousPixel { get; internal set; }
    
    /// <summary>
    /// Gets the color of the pixel.
    /// </summary>
    public Color Color { get; internal set; }
    
    /// <summary>
    /// Gets the pixel's X coordinate.
    /// </summary>
    public int X { get; internal set; }
    
    /// <summary>
    /// Gets the pixel's Y coordinate.
    /// </summary>
    public int Y { get; internal set; }

    /// <summary>
    /// Whether or not this is the first pixel on a line.
    /// </summary>
    public bool IsStart => PreviousPixel is null;
    
    /// <summary>
    /// Whether or not this is the last pixel on a line.
    /// </summary>
    public bool IsEnd => NextPixel is null;
}