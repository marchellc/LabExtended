using System.Drawing;
using System.Drawing.Imaging;

namespace LabExtended.ImageConvertor;

/// <summary>
/// Image utilities.
/// </summary>
public static class ImageUtils
{
    /// <summary>
    /// Gets the delay between each GIF frame (in milliseconds).
    /// </summary>
    /// <param name="image">The GIF image.</param>
    /// <returns>The time delay (in milliseconds).</returns>
    public static float GetFrameDelay(this Image image)
    {
        var delayPropertyBytes = image.GetPropertyItem(20736).Value;
        return BitConverter.ToInt32(delayPropertyBytes, 0) * 10;
    }
    
    /// <summary>
    /// Whether or not a given image is animated.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>true if the image is animated.</returns>
    public static bool IsAnimated(this Image image)
        => image.FrameDimensionsList.Contains(FrameDimension.Time.Guid);
}