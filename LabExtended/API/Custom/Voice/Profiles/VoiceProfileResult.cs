namespace LabExtended.API.Custom.Voice.Profiles;

/// <summary>
/// Defines the result of a voice profile operation.
/// </summary>
public enum VoiceProfileResult
{
    /// <summary>
    /// Do not modify anything.
    /// </summary>
    None,
    
    /// <summary>
    /// Skip other voice profiles and send the message.
    /// </summary>
    SkipAndSend,

    /// <summary>
    /// Skip other voice profiles and DO NOT send the message.
    /// </summary>
    SkipAndDontSend,
}