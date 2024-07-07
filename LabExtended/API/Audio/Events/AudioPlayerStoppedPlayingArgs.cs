using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.API.Audio.Events
{
    public class AudioPlayerStoppedPlayingArgs : IHookEvent
    {
        public AudioPlayer Player { get; }

        internal AudioPlayerStoppedPlayingArgs(AudioPlayer player)
            => Player = player;
    }
}