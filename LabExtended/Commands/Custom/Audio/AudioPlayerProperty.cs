using LabExtended.API.Audio;

namespace LabExtended.Commands.Custom.Audio;

/// <summary>
/// Represents a property in an audio player.
/// </summary>
public enum AudioPlayerProperty : byte
{
    /// <summary>
    /// <see cref="AudioPlayer.IsLooping"/>
    /// </summary>
    IsLooping,
    
    /// <summary>
    /// <see cref="AudioPlayer.IsPaused"/>
    /// </summary>
    IsPaused,
    
    /// <summary>
    /// <see cref="AudioPlayer.Volume"/>
    /// </summary>
    Volume,
    
    /// <summary>
    /// <see cref="AudioPlayer.NextClip"/>
    /// </summary>
    NextClip
}