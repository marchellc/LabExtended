namespace LabExtended.API.CustomVoice.Threading;

/// <summary>
/// Used as a method to call.
/// </summary>
public delegate void VoiceThreadActionDelegate(ref VoiceThreadPacket packet);

/// <summary>
/// A custom-delegate voice thread action.
/// </summary>
public class VoiceThreadAction : IVoiceThreadAction
{
    /// <summary>
    /// The delegate to invoke.
    /// </summary>
    public volatile VoiceThreadActionDelegate Delegate;
    
    /// <inheritdoc cref="IVoiceThreadAction.Modify"/>
    public void Modify(ref VoiceThreadPacket packet)
    {  
        if (Delegate is null)
            return;
        
        Delegate(ref packet);
    }
}