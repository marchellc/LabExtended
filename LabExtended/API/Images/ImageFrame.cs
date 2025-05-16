using NorthwoodLib.Pools;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Images;

/// <summary>
/// Gets the singular frame of an image.
/// </summary>
public class ImageFrame : IDisposable
{
    internal string hintFrameData = string.Empty;
    internal string toyFrameFormat = string.Empty;
    
    internal List<string> toyFrameData = ListPool<string>.Shared.Rent();
    
    /// <summary>
    /// Gets the image that this frame belongs to.
    /// </summary>
    public ImageFile File { get; internal set; }
    
    /// <summary>
    /// Gets the previous image frame.
    /// </summary>
    public ImageFrame? PreviousFrame { get; internal set; }
    
    /// <summary>
    /// Gets the next image frame.
    /// </summary>
    public ImageFrame? NextFrame { get; internal set; }
    
    /// <summary>
    /// Gets the frame's pixels.
    /// </summary>
    public List<List<ImagePixel>> Pixels { get; internal set; } = ListPool<List<ImagePixel>>.Shared.Rent();
    
    /// <summary>
    /// Gets the frame's index.
    /// </summary>
    public int Index { get; internal set; }

    /// <inheritdoc cref="ImageFile.GetRemainingDuration"/>
    public float GetRemainingDuration()
        => File.GetRemainingDuration(Index);
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Pixels != null)
        {
            Pixels.ForEach(x =>
            {
                ListPool<ImagePixel>.Shared.Return(x);
            });
            
            ListPool<List<ImagePixel>>.Shared.Return(Pixels);
        }

        if (toyFrameData != null)
            ListPool<string>.Shared.Return(toyFrameData);

        Pixels = null;
        
        toyFrameData = null;
        hintFrameData = null;
    }
}