namespace LabExtended.Images.Playback;

/// <summary>
/// Describes the state of a playback.
/// </summary>
public enum PlaybackState : byte
{
    /// <summary>
    /// The playback is not doing anything.
    /// </summary>
    Idle,
    
    /// <summary>
    /// The playback is waiting for the next frame transition.
    /// </summary>
    Waiting,
    
    /// <summary>
    /// The playback is currently displaying a frame.
    /// </summary>
    Playing,
    
    /// <summary>
    /// The playback object has been disposed
    /// </summary>
    Disposed
}