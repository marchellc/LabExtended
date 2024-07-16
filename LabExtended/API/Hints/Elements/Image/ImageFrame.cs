using System.Drawing;
using LabExtended.Utilities.Image;

namespace LabExtended.API.Hints.Elements.Image
{
    public class ImageFrame
    {
        public Bitmap Bitmap { get; }
        public string Text { get; }

        public ImageFrame(Bitmap bitmap)
        {
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            Bitmap = bitmap;
            Text = bitmap.ToHintText();
        }
    }
}