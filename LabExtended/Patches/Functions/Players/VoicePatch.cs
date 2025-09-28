using LabExtended.API;

using LabExtended.Core;
using LabExtended.Utilities;

using Mirror;

using PlayerRoles.Voice;

using VoiceChat;
using VoiceChat.Networking;

using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API.Custom.Voice.Threading;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Provides patches and events for customizing voice message handling in the voice communication system.
    /// </summary>
    public static class VoicePatch
    {
        /// <summary>
        /// Gets the event that is raised when a voice message is being received by the transceiver.
        /// </summary>
        /// <remarks>Subscribers can use this event to handle or inspect incoming voice messages as they
        /// are received. Event handlers receive a VoiceMessageReceiving argument containing details about the message.
        /// This event is static and applies to all instances of VoiceTransceiver.</remarks>
        public static FastEvent<VoiceTransceiver.VoiceMessageReceiving> OnReceiving { get; } =
            FastEvents.DefineEvent<VoiceTransceiver.VoiceMessageReceiving>(typeof(VoiceTransceiver),
                nameof(VoiceTransceiver.OnVoiceMessageReceiving));
        
        [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
        private static bool Prefix(VoiceMessage msg, NetworkConnection conn)
        {
            if (msg.SpeakerNull || msg.Speaker.netId != conn.identity.netId
                || msg.Speaker.roleManager.CurrentRole is not IVoiceRole voiceRole
                || ApiLoader.ApiConfig.VoiceSection.CustomRateLimit > 0 && voiceRole.VoiceModule._sentPackets++ >= ApiLoader.ApiConfig.VoiceSection.CustomRateLimit
                || VoiceChatMutes.IsMuted(msg.Speaker))
                return false;

            if (!ExPlayer.TryGet(msg.Speaker, out var speaker))
                return false;

            var sendingArgs = new PlayerSendingVoiceMessageEventArgs(ref msg);
            
            PlayerEvents.OnSendingVoiceMessage(sendingArgs);

            if (!sendingArgs.IsAllowed)
                return false;

            if (speaker.Voice.Thread is { IsDisposed: false } && (speaker.Voice.Thread.InstancePitch != 1f || VoiceThread.GlobalPitch != 1f))
            {
                speaker.Voice.Thread.ProcessPitch(ref msg);
            }
            else
            {
                speaker.Voice.ProcessMessage(ref msg);
            }

            return false;
        }
    }
}
