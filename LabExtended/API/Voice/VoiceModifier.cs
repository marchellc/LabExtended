using VoiceChat.Networking;

namespace LabExtended.API.Voice
{
    public abstract class VoiceModifier
    {
        public virtual bool IsEnabled { get; } = true;

        public abstract void Modify(ref VoiceMessage message, VoiceModule module);
    }
}