namespace LabExtended.API.CustomVoice.Threading;

public interface IVoiceThreadAction
{
    void Modify(ref VoiceThreadPacket packet);
}