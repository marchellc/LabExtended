using Common.Serialization;
using LabExtended.API;
using LabExtended.Core;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Async;
using LabExtended.Utilities.Image;
using System.Drawing;

namespace LabExtended.Commands.Debug.RaImages
{
    public class ExportImageDataCommand : CommandInfo
    {
        public override string Command => "exportimage";
        public override string Description => "Exports image data to a JSON";

        public object OnCalled(ExPlayer player, string fileName, string imageUrl, int width, int height)
        {
            var filePath = $"{ExLoader.Folder}/{fileName}";

            AsyncMethods.GetByteArrayAsync(imageUrl).Await(imageData =>
            {
                using (var stream = new MemoryStream(imageData))
                {
                    var image = System.Drawing.Image.FromStream(stream);
                    var bitmap = new Bitmap(image, new Size(width, height));

                    bitmap.SetResolution(width, height);

                    var data = bitmap.ToHintText();

                    File.WriteAllText(filePath, data);

                    player.RemoteAdminMessage($"Saved to file ({data.Length} bytes).");
                }
            });

            return "Converting ..";
        }
    }
}