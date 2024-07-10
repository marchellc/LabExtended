using System.Drawing;

namespace LabExtended.API.Hints.Elements.Image
{
    public class ImageElement : HintElement
    {
        private readonly Queue<Tuple<Bitmap, TimeSpan>> _imageQueue = new Queue<Tuple<Bitmap, TimeSpan>>();

        private ImageFrame _curImage;
        private DateTime _curDuration;

        public ImageElement(float verticalOffset, HintAlign align)
        {
            VerticalOffset = verticalOffset;
            Alignment = align;
        }

        public override int MaxCharactersPerLine { get; set; } = -1;
        public override bool IsRawDisplay { get; set; } = true;

        public void SetImage(Bitmap bitmap, TimeSpan duration, int height, int width, bool forceShow = false)
        {
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (bitmap.Width != width || bitmap.Height != height)
                ImageUtils.ResizeImage(ref bitmap, width, height);

            SetImage(bitmap, duration, forceShow);
        }

        public void SetImage(Bitmap bitmap, TimeSpan duration, bool forceShow = false)
        {
            if (bitmap is null)
                throw new ArgumentNullException(nameof(bitmap));

            if (duration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration));

            if (forceShow || _curImage is null)
            {
                _curImage = new ImageFrame(bitmap);
                _curDuration = DateTime.Now + duration;
            }
            else
            {
                _imageQueue.Enqueue(new Tuple<Bitmap, TimeSpan>(bitmap, duration));
            }
        }

        public void ClearImage(bool clearQueue = false)
        {
            if (clearQueue)
                _imageQueue.Clear();

            _curImage = null;
            _curDuration = DateTime.MinValue;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            ClearImage(true);
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            ClearImage(true);
        }

        public override void UpdateElement()
        {
            base.UpdateElement();

            if (_curImage != null)
            {
                if (DateTime.Now >= _curDuration)
                {
                    _curImage = null;
                    _curDuration = DateTime.MinValue;

                    if (_imageQueue.TryDequeue(out var nextImage))
                    {
                        _curImage = new ImageFrame(nextImage.Item1);
                        _curDuration = DateTime.Now + nextImage.Item2;
                    }
                }
            }
        }

        public override string GetContent()
            => _curImage?.Text ?? null;
    }
}