using LabExtended.API.Voice.Threading;
using LabExtended.API;

using LabExtended.Core;

using Mirror;

using PlayerRoles.Voice;

using VoiceChat;
using VoiceChat.Networking;

using CentralAuth;

using HarmonyLib;

namespace LabExtended.Patches.Functions.Players
{
    public static class VoicePatch
    {
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

            if (!ApiLoader.ApiConfig.VoiceSection.DisableCustomVoice)
            {
                if (ThreadedVoiceChat.IsRunning)
                {
                    ThreadedVoiceChat.Receive(speaker, ref msg);
                    return false;
                }

                speaker._voice.ReceiveMessage(ref msg);
                return false;
            }

            var sendChannel = voiceRole.VoiceModule.ValidateSend(msg.Channel);

            if (sendChannel is VoiceChatChannel.None)
                return false;

            voiceRole.VoiceModule.CurrentChannel = sendChannel;

            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.Mode != ClientInstanceMode.ReadyClient)
                    continue;

                if (hub.roleManager.CurrentRole is not IVoiceRole recvRole)
                    continue;

                var recvChannel = recvRole.VoiceModule.ValidateReceive(msg.Speaker, sendChannel);

                if (recvChannel is VoiceChatChannel.None)
                    continue;

                msg.Channel = recvChannel;
                hub.connectionToClient.Send(msg);
            }

            return false;
        }
    }
}
