using LabExtended.API.Toys;

namespace LabExtended.Commands.Custom.Audio;

/// <summary>
/// Represents a settable <see cref="SpeakerToy"/> property.
/// </summary>
public enum AudioSpeakerProperty : byte
{
    /// <summary>
    /// <see cref="SpeakerToy.IsSpatial"/>
    /// </summary>
    IsSpatial,
    
    /// <summary>
    /// <see cref="SpeakerToy.ControllerId"/>
    /// </summary>
    ControllerId,
    
    /// <summary>
    /// <see cref="SpeakerToy.MinDistance"/>
    /// </summary>
    MinDistance,
    
    /// <summary>
    /// <see cref="SpeakerToy.MaxDistance"/>
    /// </summary>
    MaxDistance,
    
    /// <summary>
    /// <see cref="SpeakerToy.Volume"/>
    /// </summary>
    Volume
}