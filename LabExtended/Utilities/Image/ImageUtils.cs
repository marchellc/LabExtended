using LabExtended.Utilities.Async;

using System.Drawing;
using System.Drawing.Imaging;

namespace LabExtended.Utilities.Image
{
    public static class ImageUtils
    {
        public static AsyncOperation<Bitmap[]> GetGifFramesAsync(this System.Drawing.Image image, Size? newSize = null)
        {
            return AsyncRunner.RunThreadAsync(() =>
            {
                var frameCount = image.GetFrameCount(FrameDimension.Time);
                var frameArray = new Bitmap[frameCount];

                for (int i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(FrameDimension.Time, i);

                    if (newSize.HasValue)
                    {
                        var frame = new Bitmap(image, newSize.Value);

                        frame.SetResolution(newSize.Value.Width, newSize.Value.Height);
                        frameArray[i] = frame;
                    }
                    else
                    {
                        frameArray[i] = new Bitmap(image);
                    }
                }

                return frameArray;
            });
        }

        public static void ResizeImage(ref Bitmap bitmap, int width, int height)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, bitmap.RawFormat);
                bitmap.Dispose();

                var image = System.Drawing.Image.FromStream(stream);

                bitmap = new Bitmap(image, new Size(width, height));
                bitmap.SetResolution(width, height);
            }
        }

        public static int GetFrameDelay(this System.Drawing.Image image, int imageIndex)
        {
            var delay = BitConverter.ToInt32(image.GetPropertyItem(20736).Value, imageIndex) * 10;

            if (delay < 100)
                delay = 100;

            return delay;
        }

        public static string ToHintText(this Bitmap bitmap)
        {
            var buffer = "";
            var color = Color.White;

            for (int i = 0; i < bitmap.Height; i++)
            {
                buffer += "<size=33%><line-height=75%>";

                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, i);

                    if (pixel != color)
                    {
                        buffer += $"<color={pixel.GetHex()}>";
                        color = pixel;
                    }

                    buffer += "█";
                }

                buffer += "\n";
            }

            return buffer + "</color>";
        }

        public static UnityEngine.Color?[,] ToPrimitiveColors(this Bitmap bitmap, UnityEngine.Color?[,] previousFrame = null)
        {
            var frame = new UnityEngine.Color?[bitmap.Height, bitmap.Width];

            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, i);
                    var color = new UnityEngine.Color(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, pixel == Color.Transparent ? 0f : 1f);

                    if (previousFrame != null && previousFrame[x, i] == color)
                    {
                        frame[i, x] = null;
                        continue;
                    }

                    frame[i, x] = color;
                }
            }

            return frame;
        }

        public static string GetHex(this Color color)
            => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}