using AdminToys;

using Common.Extensions;

using LabExtended.API.Collections.Locked;

using LabExtended.Utilities.Async;
using LabExtended.Utilities.Image;

using MEC;

using Mirror;

using System.Collections.Concurrent;
using System.Drawing;

using UnityEngine;

using Color = UnityEngine.Color;

namespace LabExtended.Utilities.Video
{
    public class PrimitiveVideoDisplay
    {
        private static int _idClock = 0;

        private LockedHashSet<PrimitiveObjectToy> _objects = new LockedHashSet<PrimitiveObjectToy>();
        private LockedHashSet<List<PrimitiveObjectToy>> _allObjects = new LockedHashSet<List<PrimitiveObjectToy>>();

        private PrimitiveObjectToy[][] _pixels;

        private GameObject _parent;

        private CoroutineHandle _coroutine;
        private volatile ConcurrentQueue<Color?[,]> _frames;

        private float _delay;

        public bool IsPaused { get; set; } = false;
        public bool IsPlaying => Timing.IsRunning(_coroutine);

        public Size Resolution { get; private set; }

        public Vector3 Position
        {
            get => _parent.transform.position;
            set => _parent.transform.position = value;
        }

        public Quaternion Rotation
        {
            get => _parent.transform.rotation;
            set => _parent.transform.rotation = value;
        }

        public event Action OnFinished;

        public void Pause()
            => IsPaused = true;

        public void Resume()
            => IsPaused = false;

        public void Stop()
        {
            Timing.KillCoroutines(_coroutine);
            QueueExtensions.Clear(_frames);

            _delay = 0f;

            IsPaused = false;
        }

        public void Play(int fps, IEnumerable<byte[]> frames)
        {
            Stop();

            _delay = 1f / fps;

            AsyncRunner.RunThreadAsync(() =>
            {
                AsyncRunner.RunThreadAsync(() =>
                {
                    foreach (var frame in frames)
                    {
                        using (var stream = new MemoryStream(frame))
                        {
                            var image = System.Drawing.Image.FromStream(stream);
                            var bitmap = new Bitmap(image, Resolution);

                            bitmap.SetResolution(Resolution.Width, Resolution.Height);

                            _frames.Enqueue(bitmap.ToPrimitiveColors());
                        }
                    }
                }).Await((() => _coroutine = Timing.RunCoroutine(Playback())));
            });
        }

        public void Play(int fps, IEnumerable<Bitmap> frames)
        {
            Stop();

            _delay = 1f / fps;

            AsyncRunner.RunThreadAsync(() =>
            {
                foreach (var frame in frames)
                {
                    var frameRef = frame;

                    if (frameRef.Size != Resolution)
                        ImageUtils.ResizeImage(ref frameRef, Resolution.Width, Resolution.Height);

                    _frames.Enqueue(frameRef.ToPrimitiveColors());
                }
            }).Await(() => _coroutine = Timing.RunCoroutine(Playback()));
        }

        public void Play(int fps, IEnumerable<Color?[,]> pixels)
        {
            Stop();

            _delay = 1f / fps;
            _frames.EnqueueMany(pixels);

            _coroutine = Timing.RunCoroutine(Playback());
        }

        public void SpawnDispplay(Size resolution, Vector3 position, Quaternion rotation, float scale = 1f)
        {
            DestroyDisplay();

            Resolution = resolution;

            _parent = new GameObject($"video{_idClock++}");

            var size = scale * 0.05f;
            var center = scale * 0.05f * resolution.Width / 2f;

            for (int i = resolution.Height; i > 0; i--)
            {
                var yAxis = i * 0.05f * scale;
                var list = new List<PrimitiveObjectToy>();

                _allObjects.Add(list);

                for (int x = resolution.Width; x > 0; x--)
                {
                    var toy = PrimitiveUtils.SpawnPrimitive(position, rotation, new Vector3(size, size, size), PrimitiveType.Cube, PrimitiveFlags.Visible);

                    toy.NetworkMovementSmoothing = 0;

                    toy.transform.localScale = new Vector3(size, size, size);
                    toy.transform.localPosition = new Vector3(x * 0.05f * scale - center, yAxis, 0f);
                    toy.transform.SetParent(_parent.transform);

                    list.Add(toy);
                }
            }

            _pixels = _allObjects.Select(c => c.ToArray()).ToArray();
        }

        public void SetColor(Color?[,] newPixels)
        {
            for (int row = 0; row < newPixels.GetLength(0); row++)
            {
                for (int col = 0; col < newPixels.GetLength(1); col++)
                {
                    var color = newPixels[row,col];

                    if (color == null)
                        continue;

                    _pixels[row][col].NetworkMaterialColor = color.Value;
                }
            }
        }

        public void Clear()
        {
            for (int row = 0; row < Resolution.Height; row++)
            {
                for (int col = 0; col < Resolution.Width; col++)
                {
                    _pixels[row][col].NetworkMaterialColor = Color.white;
                }
            }
        }

        public void DestroyDisplay()
        {
            foreach (var array in _pixels)
            {
                foreach (var pixel in array)
                    NetworkServer.Destroy(pixel.gameObject);
            }

            _allObjects.Clear();
            _objects.Clear();

            _pixels = null;

            if (_parent != null)
                UnityEngine.Object.Destroy(_parent);

            _parent = null;
        }

        private IEnumerator<float> Playback()
        {
            while (true)
            {
                if (IsPaused)
                    yield return Timing.WaitForOneFrame;

                if (_frames.TryDequeue(out var nextFrame))
                {
                    SetColor(nextFrame);
                    yield return Timing.WaitForSeconds(_delay);
                }
                else
                {
                    Clear();
                    OnFinished.Call();
                    yield break;
                }
            }
        }
    }
}