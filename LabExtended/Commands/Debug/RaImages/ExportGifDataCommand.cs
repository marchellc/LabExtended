using Common.Serialization;
using Common.Utilities;
using LabExtended.API;
using LabExtended.Core;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Async;
using LabExtended.Utilities.Image;

using System.Drawing;

namespace LabExtended.Commands.Debug.RaImages
{
    public class ExportGifDataCommand : CommandInfo
    {
        public override string Command => "exportgif";
        public override string Description => "Exports GIF data to a JSON";

        public object OnCalled(ExPlayer player, string fileName, string imageUrl, int width, int height)
        {
            var filePath = $"{ExLoader.Folder}/{fileName}";

            AsyncMethods.GetByteArrayAsync(imageUrl).Await(imageData =>
            {
                AsyncRunner.RunThreadAsync(() =>
                {
                    using (var stream = new MemoryStream(imageData))
                    {
                        var image = System.Drawing.Image.FromStream(stream);

                        image.GetGifFramesAsync(new Size(width, height)).Await(frames =>
                        {
                            var data = frames.Select(f => f.ToHintText()).ToArray().JsonSerialize();

                            File.WriteAllText(filePath, data);

                            player.RemoteAdminMessage($"Saved to file ({data.Length} bytes).");
                        });
                    }
                });
            });

            return "Converting ..";
        }
    }
}