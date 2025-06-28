using LabExtended.API.Images.Conversion;

using NorthwoodLib.Pools;
using UnityEngine;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Images;

/// <summary>
/// Represents a custom-format image file.
/// </summary>
public class ImageFile : IDisposable
{
    internal Vector2 toyDisplaySize;
    internal ToyStringImageConvertor.ToyStringImageData toyStringImageData = new();
    
    /// <summary>
    /// Gets the list of frames that this image contains.
    /// </summary>
    public List<ImageFrame> Frames { get; private set; } = ListPool<ImageFrame>.Shared.Rent();
    
    /// <summary>
    /// Gets the date of this image creation.
    /// </summary>
    public DateTime CreatedAt { get; internal set; }

    /// <summary>
    /// Gets the path to the image file.
    /// </summary>
    public string Path { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the name of the image file.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the original file extension.
    /// </summary>
    public string Extension { get; internal set; } = string.Empty;
    
    /// <summary>
    /// Gets the height of the image (in pixel count).
    /// </summary>
    public int Height { get; internal set; }
    
    /// <summary>
    /// Gets the width of the image (in pixel count).
    /// </summary>
    public int Width { get; internal set; }
    
    /// <summary>
    /// Gets the amount of milliseconds required for each frame.
    /// </summary>
    public float FrameDuration { get; internal set; }

    /// <summary>
    /// Gets the total duration of the image (in seconds).
    /// </summary>
    public float Duration => (FrameDuration * Frames.Count) / 1000f;
    
    /// <summary>
    /// Whether or not the image has any frames.
    /// </summary>
    public bool IsEmpty => Frames.Count == 0;
    
    /// <summary>
    /// Whether or not the image has only one frame.
    /// </summary>
    public bool IsStatic => Frames.Count == 1;
    
    /// <summary>
    /// Whether or not the image has multiple frames.
    /// </summary>
    public bool IsAnimated => Frames.Count > 1;
    
    /// <summary>
    /// Whether or not the file has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the remaining duration at a specific frame index.
    /// </summary>
    /// <param name="frameIndex">The index of the current frame.</param>
    /// <returns>The remaining playback duration (in seconds).</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public float GetRemainingDuration(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));

        return FrameDuration * (Frames.Count - frameIndex - 1);
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        
        IsDisposed = true;
        
        ImageLoader.OnUnloaded(this);
        
        if (Frames != null)
        {
            Frames.ForEach(f => f.Dispose());
            
            ListPool<ImageFrame>.Shared.Return(Frames);
        }

        Frames = null;
    }

    internal void ConvertFormats()
    {
        HintStringImageConvertor.ConvertImage(this);
        ToyStringImageConvertor.ConvertImage(this);
    }
}