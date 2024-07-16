using Common.Extensions;

using LabExtended.Utilities.Async;
using LabExtended.Utilities.Image;

using MEC;

using System.Drawing;

namespace LabExtended.Utilities.Video
{
    public class VideoDisplay
    {
        private CoroutineHandle _coroutine;

        private Action<string> _setFrame;
        private Queue<string> _frames;

        private float _delay;

        public bool IsPaused { get; set; } = false;
        public bool IsPlaying => Timing.IsRunning(_coroutine);

        public event Action OnFinished;

        public VideoDisplay(Action<string> setFrame)
        {
            if (setFrame is null)
                throw new ArgumentNullException(nameof(setFrame));

            _frames = new Queue<string>();
            _setFrame = setFrame;
        }

        public void PlayGifFromUrl(int fps, string url, Size? newSize = null)
        {
            Stop();

            AsyncMethods.GetByteArrayAsync(url).Await(imageData =>
            {
                using (var stream = new MemoryStream(imageData))
                {
                    var image = System.Drawing.Image.FromStream(stream);

                    image.GetGifFramesAsync(newSize).Await(frames =>
                    {
                        foreach (var frame in frames)
                            _frames.Enqueue(frame.ToHintText());

                        _delay = 1f / fps;
                        _coroutine = Timing.RunCoroutine(Playback());
                    });
                }
            });
        }

        public void PlayGifFromData(int fps, byte[] data, Size? newSize = null)
        {
            Stop();

            using (var stream = new MemoryStream(data))
            {
                var image = System.Drawing.Image.FromStream(stream);

                image.GetGifFramesAsync(newSize).Await(frames =>
                {
                    foreach (var frame in frames)
                        _frames.Enqueue(frame.ToHintText());

                    _delay = 1f / fps;
                    _coroutine = Timing.RunCoroutine(Playback());
                });
            }
        }

        public void Play(int fps, IEnumerable<byte[]> frames, Size? newSize = null)
        {
            Stop();

            var list = new List<string>();

            AsyncRunner.RunThreadAsync(() =>
            {
                foreach (var frameData in frames)
                {
                    using (var stream = new MemoryStream(frameData))
                    {
                        if (newSize.HasValue)
                        {
                            var image = System.Drawing.Image.FromStream(stream);
                            var bitmap = new Bitmap(image, newSize.Value);

                            bitmap.SetResolution(newSize.Value.Width, newSize.Value.Height);

                            list.Add(bitmap.ToHintText());
                        }
                        else
                        {
                            list.Add(new Bitmap(stream).ToHintText());
                        }
                    }
                }
            }).Await(() =>
            {
                _frames.EnqueueMany(list);
                _delay = 1f / fps;
                _coroutine = Timing.RunCoroutine(Playback());
            });
        }

        public void Play(int fps, IEnumerable<Bitmap> frames, Size? newSize = null)
        {
            Stop();

            var list = new List<string>();

            AsyncRunner.RunThreadAsync(() =>
            {
                foreach (var frame in frames)
                {
                    var nextFrame = frame;

                    if (newSize.HasValue)
                        ImageUtils.ResizeImage(ref nextFrame, newSize.Value.Width, newSize.Value.Height);

                    list.Add(nextFrame.ToHintText());
                }
            }).Await(() =>
            {
                _frames.EnqueueMany(list);
                _delay = 1f / fps;
                _coroutine = Timing.RunCoroutine(Playback());
            });
        }

        public void Play(int fps, IEnumerable<string> frames)
        {
            Stop();

            _frames.EnqueueMany(frames);
            _delay = 1f / fps;
            _coroutine = Timing.RunCoroutine(Playback());
        }

        public void Pause()
            => IsPaused = true;

        public void Resume()
            => IsPaused = false;

        public void Stop()
        {
            Timing.KillCoroutines(_coroutine);

            _frames.Clear();
            _delay = 0f;

            IsPaused = false;
        }

        private IEnumerator<float> Playback()
        {
            while (true)
            {
                if (IsPaused)
                    yield return Timing.WaitForOneFrame;

                if (_frames.TryDequeue(out var frameText))
                {
                    _setFrame.Call(frameText);
                    yield return Timing.WaitForSeconds(_delay);
                }
                else
                {
                    OnFinished.Call();
                    yield break;
                }
            }
        }
    }
}