using LabExtended.API;

using LabExtended.Extensions;

using VoiceChat.Networking;

namespace LabExtended.Events;

public delegate void ReceivingVoiceMessageHandler(ExPlayer player, ExPlayer receiver, ref VoiceMessage message);
public delegate void SendingVoiceMessageHandler(ExPlayer speaker, ref VoiceMessage message);

public class VoiceEvents
{
    public static event ReceivingVoiceMessageHandler OnReceivingVoiceMessage;
    public static event SendingVoiceMessageHandler OnSendingVoiceMessage;
    
    public static event Action<ExPlayer> OnStartedSpeaking;
    public static event Action<ExPlayer, float, Dictionary<DateTime, VoiceMessage>> OnStoppedSpeaking;

    public static void InvokeOnStartedSpeaking(ExPlayer player)
        => OnStartedSpeaking.InvokeSafe(player);
    
    public static void InvokeOnStoppedSpeaking(ExPlayer player, float time, Dictionary<DateTime, VoiceMessage> packets)
        => OnStoppedSpeaking.InvokeSafe(player, time, packets);
    
    public static void InvokeOnReceiving(ExPlayer player, ExPlayer receiver, ref VoiceMessage message)
        => OnReceivingVoiceMessage?.Invoke(player, receiver, ref message);
    
    public static void InvokeOnSending(ExPlayer player, ref VoiceMessage message)
        => OnSendingVoiceMessage?.Invoke(player, ref message);
}