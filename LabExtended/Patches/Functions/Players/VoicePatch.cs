using LabExtended.API;

using LabExtended.Core;

using Mirror;

using PlayerRoles.Voice;

using VoiceChat;
using VoiceChat.Networking;

using CentralAuth;

using HarmonyLib;

using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Players
{
    public static class VoicePatch
    {
        public static event Action<ExPlayer, VoiceMessage> OnMessage;
        
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

            OnMessage.InvokeSafe(speaker, msg);
            
            speaker.Voice.Pitch.ProcessMessage(ref msg);
            return false;
        }
    }
}
