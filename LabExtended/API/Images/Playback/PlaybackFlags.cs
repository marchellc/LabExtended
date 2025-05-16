namespace LabExtended.Images.Playback;

/// <summary>
/// Configurable playback option flags.
/// </summary>
[Flags]
public enum PlaybackFlags : byte
{
    /// <summary>
    /// No options.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Loop the current image.
    /// </summary>
    Loop = 1,
    
    /// <summary>
    /// Pauses the playback.
    /// </summary>
    Pause = 2
}