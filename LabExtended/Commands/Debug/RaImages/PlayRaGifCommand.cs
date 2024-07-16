using Common.Serialization;
using Common.Utilities;
using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Async;
using LabExtended.Utilities.Video;

using MEC;

namespace LabExtended.Commands.Debug.Video
{
    public class PlayRaGifCommand : CommandInfo
    {
        private static readonly VideoDisplay _raDisplay = new VideoDisplay(frame => _raPlayer.RemoteAdminInfo(frame));
        private static ExPlayer _raPlayer;

        public override string Command => "ragif";
        public override string Description => "Plays a GIF in the remote admin.";

        public object OnCalled(ExPlayer player, int fps, string url)
        {
            Timing.CallDelayed(3f, () =>
            {
                AsyncMethods.GetStringAsync(url).Await(data =>
                {
                    player.RemoteAdminMessage($"Downloaded {data.Length} bytes");

                    var frames = data.JsonDeserialize<string[]>();

                    _raPlayer = player;
                    _raDisplay.Play(fps, frames);
                });
            });

            return "Playing ..";
        }
    }
}