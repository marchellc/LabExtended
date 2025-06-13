using Achievements.Handlers;

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
using LabExtended.API.CustomVoice.Threading;

namespace LabExtended.Patches.Functions.Players
{
    public static class VoicePatch
    {
        public static FastEvent<VoiceTransceiver.VoiceMessageReceiving> OnReceiving { get; } =
            FastEvents.DefineEvent<VoiceTransceiver.VoiceMessageReceiving>(typeof(VoiceTransceiver),
                nameof(VoiceTransceiver.OnVoiceMessageReceiving));
        
        [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
        public static bool Prefix(VoiceMessage msg, NetworkConnection conn)
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
