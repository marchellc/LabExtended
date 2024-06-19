using LabExtended.API;
using LabExtended.API.Voice;
using LabExtended.Core;

using Mirror;

using PlayerRoles.Voice;

using VoiceChat;
using VoiceChat.Networking;

using CentralAuth;

using HarmonyLib;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoicePatch
    {
        public static bool Prefix(VoiceMessage msg, NetworkConnection conn)
        {
            if (msg.SpeakerNull || msg.Speaker.netId != conn.identity.netId
                || msg.Speaker.roleManager.CurrentRole is not IVoiceRole voiceRole
                || (ExLoader.Loader.Config.Voice.CustomRateLimit > 0 && voiceRole.VoiceModule._sentPackets++ >= ExLoader.Loader.Config.Voice.CustomRateLimit)
                || VoiceChatMutes.IsMuted(msg.Speaker))
                return false;

            var speaker = ExPlayer.Get(msg.Speaker);

            if (speaker is null)
                return true;

            if (!ExLoader.Loader.Config.Voice.DisableCustomVoice)
            {
                VoiceSystem.Receive(ref msg, speaker);
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
