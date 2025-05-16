using LabExtended.API.Images;

namespace LabExtended.Images.Playback;

/// <summary>
/// Represents an object that can display images.
/// </summary>
public interface IPlaybackDisplay : IDisposable
{
    /// <summary>
    /// Gets the image playback display.
    /// </summary>
    PlaybackBase PlaybackDisplay { get; }

    /// <summary>
    /// Sets the display's current frame.
    /// </summary>
    /// <param name="frame">The frame to set (or null if to clear the display).</param>
    void SetFrame(ImageFrame? frame);
}