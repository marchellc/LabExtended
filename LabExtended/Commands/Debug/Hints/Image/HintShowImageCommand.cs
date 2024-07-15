using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.Hints.Elements.Image;

using LabExtended.Core.Commands;
using LabExtended.Utilities.Async;

using System.Drawing;
using System.Net.Http;

namespace LabExtended.Commands.Debug.Hints.Image
{
    public class HintShowImageCommand : CommandInfo
    {
        public override string Command => "showimage";
        public override string Description => "Shows an image from a URL.";

        public object OnCalled(ExPlayer sender, string url, int duration, int width, int height)
        {
            var op = AsyncRunner.RunAsync(DownloadImage(url, width, height));

            op.Await(bitmap =>
            {
                if (op.Error != null)
                    sender.RemoteAdminMessage($"Error: {op.Error}");

                if (!sender.Hints.TryGetElement<ImageElement>(out var imageElement))
                    imageElement = sender.Hints.AddElement(new ImageElement(0f, HintAlign.Center));

                ImageUtils.ResizeImage(ref bitmap, width, height);
                imageElement.SetImage(bitmap, TimeSpan.FromSeconds(duration), true);

                sender.RemoteAdminMessage("Downloaded");
            });

            return "Downloading image ..";
        }

        private async Task<Bitmap> DownloadImage(string url, int width, int height)
        {
            using (var client = new HttpClient())
            {
                using (var stream = new MemoryStream(await client.GetByteArrayAsync(url)))
                    return new Bitmap(System.Drawing.Image.FromStream(stream), new Size(width, height));
            }
        }
    }
}