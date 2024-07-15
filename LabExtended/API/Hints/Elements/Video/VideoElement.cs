using Common.Extensions;

using LabExtended.API.Enums;
using LabExtended.API.Hints.Elements.Image;
using LabExtended.Utilities.Async;

using MEC;

using System.Collections.Concurrent;
using System.Drawing;

namespace LabExtended.API.Hints.Elements.Video
{
    public class VideoElement : HintElement
    {
        private readonly Queue<Tuple<List<VideoFrame>, float>> _videoQueue = new Queue<Tuple<List<VideoFrame>, float>>();

        private volatile ConcurrentQueue<VideoFrame> _frameQueue = new ConcurrentQueue<VideoFrame>();
        private volatile VideoFrame _curFrame;

        private TimeSpan _frameDuration;
        private DateTime _lastFrameTime;

        public bool IsPlaying => _curFrame != null;
        public bool IsPaused { get; set; }

        public VideoFrame Frame => _curFrame;

        public override int MaxCharactersPerLine { get; set; } = -1;

        public override bool IsRawDisplay { get; set; } = true;

        public VideoElement(float verticalOffset, HintAlign alignment)
        {
            VerticalOffset = verticalOffset;
            Alignment = alignment;
        }

        public void Pause()
            => IsPaused = true;

        public void Resume()
            => IsPaused = false;

        public void Stop(bool clearQueue = false)
        {
            if (clearQueue)
                _videoQueue.Clear();

            _frameQueue.Clear();
            _curFrame = null;
        }

        public void Skip()
        {
            _frameQueue.Clear();
            _curFrame = null;

            if (_videoQueue.TryDequeue(out var frames))
            {
                _lastFrameTime = DateTime.MinValue;

                _frameDuration = TimeSpan.FromMilliseconds(frames.Item2);
                _frameQueue.EnqueueMany(frames.Item1);
            }
        }

        public void Play(Bitmap[] frames, float frameRate, bool forcePlay = false)
        {
            if (frames is null)
                throw new ArgumentNullException(nameof(frames));

            ConvertFrames(frames, list => Play(list, frameRate, forcePlay));
        }

        public void Play(IEnumerable<VideoFrame> convertedFrames, float frameRate, bool forcePlay = false)
        {
            if (convertedFrames is null)
                throw new ArgumentNullException(nameof(convertedFrames));

            if (_curFrame is null || forcePlay)
            {
                _lastFrameTime = DateTime.MinValue;

                _frameDuration = TimeSpan.FromMilliseconds(frameRate);
                _frameQueue.EnqueueMany(convertedFrames);
            }
            else
            {
                _videoQueue.Enqueue(new Tuple<List<VideoFrame>, float>(convertedFrames.ToList(), frameRate));
            }
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Stop(true);
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            Stop(true);
        }

        public override void UpdateElement()
        {
            base.UpdateElement();

            if (!IsPaused)
            {
                if (_curFrame != null)
                {
                    if ((DateTime.Now - _lastFrameTime) < _frameDuration)
                        return;

                    if (_frameQueue.TryDequeue(out var nextFrame))
                    {
                        _curFrame = nextFrame;
                        _lastFrameTime = DateTime.Now;
                    }
                    else
                    {
                        _curFrame = null;

                        _lastFrameTime = DateTime.MinValue;
                        _frameDuration = TimeSpan.Zero;

                        if (_videoQueue.TryDequeue(out var next))
                        {
                            _frameDuration = TimeSpan.FromMilliseconds(next.Item2);
                            _frameQueue.EnqueueMany(next.Item1);
                        }
                    }
                }
                else
                {
                    if (_videoQueue.TryDequeue(out var next))
                    {
                        _frameDuration = TimeSpan.FromMilliseconds(next.Item2);
                        _frameQueue.EnqueueMany(next.Item1);
                    }
                    else if (_frameQueue.TryDequeue(out var nextFrame))
                    {
                        _curFrame = nextFrame;
                        _lastFrameTime = DateTime.Now;
                    }
                }
            }
        }

        public override string GetContent()
            => _curFrame?.Text ?? null;

        public static void ConvertFrames(Bitmap[] frames, Action<List<VideoFrame>> callback)
            => Timing.RunCoroutine(InternalConvertFramesCoroutine(frames, callback));

        private static IEnumerator<float> InternalConvertFramesCoroutine(Bitmap[] frames, Action<List<VideoFrame>> callback)
        {
            var addOp = AsyncRunner.RunThreadAsync(() =>
            {
                var list = new List<VideoFrame>();

                for (int i = 0; i < frames.Length; i++)
                {
                    var frame = frames[i];
                    list.Add(new VideoFrame(frame, frame.ToHintText()));
                }

                return list;
            });

            while (!addOp.IsDone)
                yield return Timing.WaitForSeconds(0.1f);

            callback.Call(addOp.Result);
        }
    }
}