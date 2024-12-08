using VoiceChat;

namespace LabExtended.API.Voice.Threading
{
    public class ThreadedVoicePacket : IDisposable
    {
        public volatile VoiceChatChannel Channel;
        public volatile ExPlayer Speaker;
        public volatile byte[] Data;
        public volatile int Size;

        public void Dispose()
        {
            Channel = default;
            Speaker = null;
            Data = null;
            Size = 0;
        }
    }
}