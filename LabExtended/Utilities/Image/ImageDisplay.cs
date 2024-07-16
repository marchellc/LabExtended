using Common.Extensions;

using LabExtended.Utilities.Async;

using System.Drawing;

namespace LabExtended.Utilities.Image
{
    public static class ImageDisplay
    {
        public static void DisplayImageFromUrl(string url, Size? newSize, Action<string> display)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            if (display is null)
                throw new ArgumentNullException(nameof(display));

            AsyncMethods.GetByteArrayAsync(url).Await(data => DisplayImage(data, newSize, display));
        }

        public static void DisplayImageFromFile(string filePath, Size? newSize, Action<string> display)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (display is null)
                throw new ArgumentNullException(nameof(display));

            DisplayImage(File.ReadAllBytes(filePath), newSize, display);
        }

        public static void DisplayImage(byte[] imageData, Size? newSize, Action<string> display)
        {
            if (imageData is null)
                throw new ArgumentNullException(nameof(imageData));

            if (display is null)
                throw new ArgumentNullException(nameof(display));

            using (var stream = new MemoryStream(imageData))
                DisplayImage(newSize.HasValue
                    ? new Bitmap(System.Drawing.Image.FromStream(stream), newSize.Value)
                    : new Bitmap(stream), newSize, display);
        }

        public static void DisplayImage(Bitmap image, Size? newSize, Action<string> display)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            if (display is null)
                throw new ArgumentNullException(nameof(display));

            if (newSize.HasValue)
                ImageUtils.ResizeImage(ref image, newSize.Value.Width, newSize.Value.Height);

            display.Call(image.ToHintText());
        }

        public static void DisplayImage(string image, Action<string> display)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            if (display is null)
                throw new ArgumentNullException(nameof(display));

            display.Call(image);
        }
    }
}