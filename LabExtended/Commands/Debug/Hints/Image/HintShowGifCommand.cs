using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.Hints.Elements.Image;
using LabExtended.API.Hints.Elements.Video;

using LabExtended.Core.Commands;
using LabExtended.Utilities.Async;

using System.Net.Http;

namespace LabExtended.Commands.Debug.Hints.Image
{
    public class HintShowGifCommand : CommandInfo
    {
        public override string Command => "showgif";
        public override string Description => "Shows an gif from a URL.";

        public object OnCalled(ExPlayer sender, string url, float frameDelay, int width, int height)
        {
            var op = AsyncRunner.RunThreadAsync(DownloadImage(url, width, height));

            op.Await(image =>
            {
                try
                {
                    if (op.Error != null)
                        sender.RemoteAdminMessage($"Error: {op.Error}");

                    if (!sender.Hints.TryGetElement<VideoElement>(out var videoElement))
                        videoElement = sender.Hints.AddElement(new VideoElement(0f, HintAlign.Center));

                    sender.RemoteAdminMessage("Downloaded");

                    ImageUtils.GetGifFramesAsync(image, new System.Drawing.Size(width, height)).Await(frames =>
                    {
                        sender.RemoteAdminMessage($"Downloaded {frames.Length} frames");
                        videoElement.Play(frames, frameDelay, true);
                    });
                }
                catch (Exception ex)
                {
                    sender.RemoteAdminMessage(ex);
                }
            });

            return "Downloading image ..";
        }

        private async Task<System.Drawing.Image> DownloadImage(string url, int width, int height)
        {
            using (var client = new HttpClient())
            using (var stream = new MemoryStream(await client.GetByteArrayAsync(url)))
                return System.Drawing.Image.FromStream(stream);
        }
    }
}