namespace LabExtended.API.Custom.Voice.Threading;

/// <summary>
/// A common interface for voice thread actions.
/// </summary>
public interface IVoiceThreadAction
{
    /// <summary>
    /// Gets called when a packet is to be modified.
    /// </summary>
    /// <param name="packet">The packet to modify.</param>
    void Modify(ref VoiceThreadPacket packet);
}