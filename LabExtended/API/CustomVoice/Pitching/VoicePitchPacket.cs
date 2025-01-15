using VoiceChat;
using VoiceChat.Codec;

namespace LabExtended.API.CustomVoice.Pitching;

public class VoicePitchPacket
{
    public volatile IVoicePitchAction Action;

    public volatile byte[] Data;
    public volatile int Length;
    public volatile float Pitch;

    public volatile ExPlayer Speaker;
    public volatile VoiceController Controller;

    public volatile OpusEncoder Encoder;
    public volatile OpusDecoder Decoder;
    
    public volatile Action<VoicePitchPacket> OnProcessed;

    public volatile VoiceChatChannel OriginalChannel;
}