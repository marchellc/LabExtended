using LabExtended.API;
using LabExtended.API.CustomVoice;

using LabExtended.Extensions;

using VoiceChat.Networking;

namespace LabExtended.Events;

/// <summary>
/// Voice chat related events.
/// <remarks>Voice chat events are called A LOT, which is why there aren't any event argument classes.</remarks>
/// </summary>
public static class ExVoiceChatEvents
{
    /// <summary>
    /// Used to handle the <see cref="ExVoiceChatEvents.ReceivingVoiceMessage"/> event.
    /// </summary>
    public delegate void ReceivingVoiceMessageEventHandler(ExPlayer player, ExPlayer receiver, ref VoiceMessage message);
    
    /// <summary>
    /// Used to handle the <see cref="ExVoiceChatEvents.SendingVoiceMessage"/> event.
    /// </summary>
    public delegate void SendingVoiceMessageEventHandler(ExPlayer speaker, ref VoiceMessage message);

    /// <summary>
    /// Used to handle the <see cref="ExVoiceChatEvents.StoppedSpeaking"/> event.
    /// <remarks>speakingDuration is in seconds.</remarks>
    /// <remarks>Packets are not captured unless the player's packet capture is enabled (via <see cref="VoiceController.IsCapturing"/>)</remarks>
    /// </summary>
    public delegate void StoppedSpeakingEventHandler(ExPlayer speaker, float speakingDuration,
        Dictionary<long, VoiceMessage>? sentPackets);
    
    /// <summary>
    /// Gets called once a <see cref="VoiceMessage"/> is received from a player.
    /// </summary>
    public static event ReceivingVoiceMessageEventHandler? ReceivingVoiceMessage;
    
    /// <summary>
    /// Gets called once a <see cref="VoiceMessage"/> is processed and being sent to players.
    /// </summary>
    public static event SendingVoiceMessageEventHandler? SendingVoiceMessage;
    
    /// <summary>
    /// Gets called when a player starts speaking.
    /// </summary>
    public static event Action<ExPlayer>? StartedSpeaking;
    
    /// <summary>
    /// Gets called when a player stops speaking.
    /// <remarks>Packets are not captured unless the player's packet capture is enabled (via <see cref="VoiceController.IsCapturing"/>)</remarks>
    /// </summary>
    public static event StoppedSpeakingEventHandler? StoppedSpeaking;

    /// <summary>
    /// Executes the <see cref="StartedSpeaking"/> event.
    /// </summary>
    /// <param name="player">The player who started speaking.</param>
    public static void OnStartedSpeaking(ExPlayer player)
        => StartedSpeaking?.InvokeSafe(player);
    
    /// <summary>
    /// Executes the <see cref="StoppedSpeaking"/> event.
    /// <remarks>Packets are not captured unless the player's packet capture is enabled (via <see cref="VoiceController.IsCapturing"/>)</remarks>
    /// </summary>
    /// <param name="player">The player who stopped speaking.</param>
    /// <param name="time">How long the player was speaking (in seconds).</param>
    /// <param name="packets">The packets that were received.</param>
    public static void OnStoppedSpeaking(ExPlayer player, float time, Dictionary<long, VoiceMessage>? packets)
        => StoppedSpeaking?.Invoke(player, time, packets);
    
    /// <summary>
    /// Executes the <see cref="ReceivingVoiceMessage"/> event.
    /// </summary>
    /// <param name="player">The player that is speaking.</param>
    /// <param name="receiver">The player who is receiving the voice message.</param>
    /// <param name="message">The voice message.</param>
    public static void OnReceivingVoiceMessage(ExPlayer player, ExPlayer receiver, ref VoiceMessage message)
        => ReceivingVoiceMessage?.InvokeEvent(player, receiver, ref message);
    
    /// <summary>
    /// Executes the <see cref="SendingVoiceMessage"/> event.
    /// </summary>
    /// <param name="player">The player that is speaking.</param>
    /// <param name="message">The voice message.</param>
    public static void OnSendingVoiceMessage(ExPlayer player, ref VoiceMessage message)
        => SendingVoiceMessage?.InvokeEvent(player, ref message);
}