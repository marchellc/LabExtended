namespace LabExtended.API.CustomVoice.Threading;

public delegate void VoiceThreadActionDelegate(ref VoiceThreadPacket packet);

public class VoiceThreadAction : IVoiceThreadAction
{
    public volatile VoiceThreadActionDelegate Delegate;
    
    public void Modify(ref VoiceThreadPacket packet)
    {  
        if (Delegate is null)
            return;
        
        Delegate(ref packet);
    }
}