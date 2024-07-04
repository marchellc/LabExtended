using LabExtended.API.Voice.Profiles;
using LabExtended.API.Voice.Processing;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Extensions;
using LabExtended.Ticking;

using LabExtended.Events.Player;

using VoiceChat.Networking;
using VoiceChat;

using Common.Pooling.Pools;
using Common.Extensions;

namespace LabExtended.API.Voice
{
    public static class VoiceSystem
    {
        internal static bool ShowSpeakingDebug; // Used in the debug command.

        static VoiceSystem()
            => TickManager.SubscribeTick(SpeakingWatcher, TickOptions.NoneSeparateProfiled);

        /// <summary>
        /// Gets a list of active voice processors.
        /// </summary>
        public static List<IVoiceProcessor> VoiceProcessors { get; } = new List<IVoiceProcessor>()
        {
            new VoicePitchProcessor()
        };

        /// <summary>
        /// Gets called when a player sends a packet after all custom processing finishes.
        /// </summary>
        public static event Action<VoiceMessage, ExPlayer, Dictionary<ExPlayer, VoiceChatChannel>, Action<ExPlayer, VoiceChatChannel>> OnPostProcessing;

        public static event Action<ExPlayer> OnStartedSpeaking;
        public static event Action<ExPlayer> OnStoppedSpeaking;

        internal static void SetProfile(ExPlayer player, VoiceProfileBase voiceProfileBase) // Implemented in ExPlayer
        {
            if (player._voiceProfile != null)
            {
                player._voiceProfile.IsActive = false;
                player._voiceProfile.OnStopped();
                player._voiceProfile.Player = null;
            }

            if (voiceProfileBase != null)
            {
                voiceProfileBase.Player = player;
                voiceProfileBase.IsActive = true;

                player._voiceProfile = voiceProfileBase;

                voiceProfileBase.OnStarted();
            }
        }

        internal static void Receive(ref VoiceMessage message, ExPlayer speaker) // Called by the voice patch
        {
            if (speaker.Role.VoiceModule is null)
                return;

            var dict = DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Rent();
            var copy = DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Rent();

            try
            {
                var list = ExPlayer._players;

                foreach (var player in list)
                {
                    if (player.Role.VoiceModule is null)
                    {
                        dict[player] = copy[player] = VoiceChatChannel.None;
                        continue;
                    }

                    if (player.NetId == speaker.NetId)
                    {
                        if (!player.VoiceFlags.HasFlag(VoiceFlags.CanHearSelf))
                        {
                            dict[player] = copy[player] = VoiceChatChannel.None;
                            continue;
                        }
                        else
                        {
                            dict[player] = copy[player] = VoiceChatChannel.RoundSummary;
                            continue;
                        }
                    }

                    if (speaker.Switches.CanBeHeard
                        || (speaker.Switches.CanBeHeardByStaff && player.HasRemoteAdminAccess)
                        || (speaker.Switches.CanBeHeardBySpectators && player.Role.IsSpectator && player.IsSpectating(speaker)))
                    {
                        if (!speaker.Switches.CanBeHeardByOtherRoles && speaker.Role.Type != player.Role.Type)
                        {
                            dict[player] = copy[player] = VoiceChatChannel.None;
                            continue;
                        }
                        else
                        {
                            dict[player] = copy[player] = player.Role.VoiceModule.ValidateReceive(speaker.Hub, message.Channel);
                            continue;
                        }
                    }
                }

                speaker._speakingCapture.Add(message.Data);
                speaker._voiceProfile?.OnReceived(copy, (player, channel) => dict[player] = channel);

                speaker.Role.VoiceModule.CurrentChannel = message.Channel;

                for (int i = 0; i < VoiceProcessors.Count; i++)
                {
                    var processor = VoiceProcessors[i];

                    if (!processor.IsGloballyActive && !processor.IsActive(speaker))
                        continue;

                    processor.Process(speaker, ref message.Data, ref message.DataLength);
                }

                try
                {
                    if (OnPostProcessing != null) // We have a listener
                    {
                        // Synchronize the changes possibly made by voice profiles
                        copy.Clear();
                        copy.AddRange(dict);

                        OnPostProcessing(message, speaker, copy, (player, channel) => dict[player] = channel);
                    }
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Voice API", $"The &3OnPostProcessing&r event caught an exception:\n{ex.ToColoredString()}");
                }

                foreach (var player in list)
                {
                    if (!dict.ContainsKey(player))
                        continue;

                    var channel = dict[player];

                    if (channel is VoiceChatChannel.None)
                        continue;

                    message.Channel = channel;
                    player.Connection.Send(message);
                }
            }
            catch (Exception ex)
            {
                ExLoader.Error("Voice API", $"The receiver function caught an error:\n{ex.ToColoredString()}");
            }

            DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Return(copy);
            DictionaryPool<ExPlayer, VoiceChatChannel>.Shared.Return(dict);
        }

        private static void SpeakingWatcher() // Called by the update event. This seems VERY inefficient ..
        {
            try
            {
                for (int i = 0; i < ExPlayer._players.Count; i++)
                {
                    try
                    {
                        var player = ExPlayer._players[i];

                        if (player.Role.VoiceModule is null)
                            continue;

                        if (player.Role.VoiceModule.ServerIsSending && !player._wasSpeaking)
                        {
                            player._wasSpeaking = true;
                            player._wasSpeakingAt = DateTime.Now;
                            player._speakingCapture = ListPool<byte[]>.Shared.Rent();
                            player._voiceProfile?.OnStartedSpeaking();

                            OnStartedSpeaking.Call(player);

                            HookRunner.RunEvent(new PlayerStartedSpeakingArgs(player));

                            if (ShowSpeakingDebug)
                                ExLoader.Debug("Voice API", $"Player &3{player.Name}&r (&6{player.UserId}&r) started speaking.");
                        }
                        else if (!player.Role.VoiceModule.ServerIsSending && player._wasSpeaking)
                        {
                            var wasSpeakingAt = player._wasSpeakingAt;
                            var endedSpeakingAt = DateTime.Now;
                            var speakingDuration = endedSpeakingAt - wasSpeakingAt;
                            var capture = ListPool<byte[]>.Shared.ToArrayReturn(player._speakingCapture);

                            player._wasSpeaking = false;
                            player._wasSpeakingAt = DateTime.MinValue;
                            player._voiceProfile?.OnStoppedSpeaking(wasSpeakingAt, speakingDuration, capture);

                            OnStoppedSpeaking.Call(player);

                            HookRunner.RunEvent(new PlayerStoppedSpeakingArgs(player, wasSpeakingAt, speakingDuration, capture));

                            if (ShowSpeakingDebug)
                                ExLoader.Debug("Voice API", $"Player &3{player.Name}&r (&6{player.UserId}&r) stopped speaking.\n" +
                                    $"- &3Started at&r &6{wasSpeakingAt}&r\n" +
                                    $"- &3Spoken for&r &6{speakingDuration}&r\n" +
                                    $"- &3Sent&r &6{capture.Length}&r &3packets&r.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Voice API", $"The update function caught an error while updating &3index={i}&r (&6{ExPlayer._players[i].UserId}&r):\n{ex.ToColoredString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                ExLoader.Error("Voice API", $"The update function caught an error:\n{ex.ToColoredString()}");
            }
        }
    }
}