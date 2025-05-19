using VoiceChat;
using VoiceChat.Codec;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.CustomVoice.Threading;

/// <summary>
/// Represents a voice packet to-be modified.
/// </summary>
public class VoiceThreadPacket
{
    /// <summary>
    /// The action used to modify the packet.
    /// </summary>
    public volatile IVoiceThreadAction Action;

    /// <summary>
    /// The packet data.
    /// </summary>
    public volatile byte[] Data;
    
    /// <summary>
    /// The packet data length.
    /// </summary>
    public volatile int Length;
    
    /// <summary>
    /// The packet pitch factor.
    /// </summary>
    public volatile float Pitch;

    /// <summary>
    /// The packet sender.
    /// </summary>
    public volatile ExPlayer Speaker;
    
    /// <summary>
    /// The voice controller of the packet sender.
    /// </summary>
    public volatile VoiceController Controller;

    /// <summary>
    /// The voice packet encoder.
    /// </summary>
    public volatile OpusEncoder Encoder;
    
    /// <summary>
    /// The voice packet decoder.
    /// </summary>
    public volatile OpusDecoder Decoder;
    
    /// <summary>
    /// The method to call once the packet is processed.
    /// </summary>
    public volatile Action<VoiceThreadPacket> OnProcessed;

    /// <summary>
    /// The original voice channel of the packet.
    /// </summary>
    public volatile VoiceChatChannel OriginalChannel;
}