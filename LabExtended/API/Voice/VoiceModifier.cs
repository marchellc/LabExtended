using LabExtended.API.Voice.Threading;

using VoiceChat.Networking;

namespace LabExtended.API.Voice
{
    public abstract class VoiceModifier
    {
        public virtual bool IsEnabled { get; } = true;
        public virtual bool IsThreaded { get; } = false;

        public abstract void ModifySafe(ref VoiceMessage message, VoiceModule module);
        public abstract void ModifyThreaded(ref ThreadedVoicePacket packet);
    }
}