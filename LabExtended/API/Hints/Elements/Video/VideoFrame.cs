using System.Drawing;

namespace LabExtended.API.Hints.Elements.Video
{
    public class VideoFrame
    {
        public volatile Bitmap Bitmap;
        public volatile string Text;

        public VideoFrame(Bitmap bitmap, string text)
        {
            Bitmap = bitmap;
            Text = text;
        }
    }
}