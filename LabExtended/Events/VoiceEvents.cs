using LabExtended.API;

using LabExtended.Extensions;

using VoiceChat.Networking;

namespace LabExtended.Events;

public class VoiceEvents
{
    public static event Action<ExPlayer> OnStartedSpeaking;
    public static event Action<ExPlayer, float, Dictionary<float, VoiceMessage>> OnStoppedSpeaking;

    public static void InvokeOnStartedSpeaking(ExPlayer player)
        => OnStartedSpeaking.InvokeSafe(player);
    
    public static void InvokeOnStoppedSpeaking(ExPlayer player, float time, Dictionary<float, VoiceMessage> packets)
        => OnStoppedSpeaking.InvokeSafe(player, time, packets);
}