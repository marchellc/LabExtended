namespace LabExtended.API.CustomVoice.Pitching;

public interface IVoicePitchAction
{
    void Modify(ref VoicePitchPacket packet);
}