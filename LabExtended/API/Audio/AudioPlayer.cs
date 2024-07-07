using Common.Caching;
using Common.Extensions;
using Common.Utilities.Generation;

using LabExtended.API.Audio.Enums;
using LabExtended.API.Audio.Events;
using LabExtended.API.Modules;
using LabExtended.Core.Hooking;
using LabExtended.Ticking;

using MEC;

using NVorbis;

using UnityEngine;

using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace LabExtended.API.Audio
{
    public class AudioPlayer : Module, IDisposable
    {
        private static readonly List<AudioPlayer> _players = new List<AudioPlayer>();
        private static readonly UniqueStringGenerator _audioId = new UniqueStringGenerator(new MemoryCache<string>(), 20, false);

        public static IReadOnlyList<AudioPlayer> Players => _players;
        public static int PlayerCount => _players.Count;

        private bool _shouldStop;
        private bool _shouldPlay;
        private bool _isReady;

        private float _allowedSamples;

        private int _samplesPerSecond;

        private CoroutineHandle _playback;

        private MemoryStream _stream;
        private VorbisReader _reader;

        public OpusEncoder Encoder { get; } = new OpusEncoder(OpusApplicationType.Voip);
        public PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer(24000, false);

        public Queue<float> StreamBuffer { get; } = new Queue<float>();

        public byte[] EncodedBuffer { get; } = new byte[512];

        public float[] SendBuffer { get; private set; }
        public float[] ReadBuffer { get; private set; }

        public AudioStatus Status { get; private set; } = AudioStatus.Idle;
        public AudioFlags Flags { get; private set; } = AudioFlags.PlayNext;

        public AudioInfo Current { get; private set; }
        public AudioInfo Next { get; private set; }
        public AudioInfo Previous { get; private set; }

        public AudioChannel Channel { get; } = new AudioChannel();

        public Queue<AudioInfo> Queue { get; } = new Queue<AudioInfo>();

        public event Action OnFinished;

        public float Volume { get; set; } = 100f;

        public bool IsPaused
        {
            get => !_shouldPlay;
            set => _shouldPlay = !value;
        }

        public override TickOptions TickOptions { get; } = TickOptions.None;

        public void Play(AudioInfo audioInfo)
        {
            if (!audioInfo.IsPlayable)
                throw new InvalidOperationException($"Audio {audioInfo.Id} cannot be played.");

            if (Current is null)
            {
                Current = audioInfo;
                _playback = Timing.RunCoroutine(Playback(), Segment.FixedUpdate);
            }
            else
            {
                Queue.Enqueue(audioInfo);
            }
        }

        public void Stop(bool clearQueue = false)
        {
            if (clearQueue)
                Queue.Clear();

            _shouldStop = true;
        }

        public void Dispose()
        {
            if (Timing.IsRunning(_playback))
                Timing.KillCoroutines(_playback);

            Stop();
        }

        public override void OnTick()
        {
            base.OnTick();

            if (!_isReady || StreamBuffer.Count < 1 || !_shouldPlay)
                return;

            _allowedSamples += Time.deltaTime * _samplesPerSecond;

            var copies = Mathf.Min(Mathf.FloorToInt(_allowedSamples), StreamBuffer.Count);

            if (copies > 0)
            {
                for (int i = 0; i < copies; i++)
                    PlaybackBuffer.Write(StreamBuffer.Dequeue() * (Volume / 100f));
            }

            _allowedSamples -= copies;

            while (PlaybackBuffer.Length >= 480)
            {
                PlaybackBuffer.ReadTo(SendBuffer, 480L, 0L);

                var length = Encoder.Encode(SendBuffer, EncodedBuffer, 480);

                foreach (var player in ExPlayer.Players)
                {
                    var channel = Channel.Get(player);

                    if (!channel.HasValue)
                        continue;

                    player.Connection.Send(new VoiceMessage((channel.Value.Speaker ?? player).Hub, channel.Value.Channel, EncodedBuffer, length, false));
                }
            }
        }

        private IEnumerator<float> Playback()
        {
            Status = AudioStatus.Idle;

            _isReady = false;
            _shouldStop = false;
            _shouldPlay = true;

            _stream = new MemoryStream(Current.Data);
            _stream.Seek(0, SeekOrigin.Begin);

            _reader = new VorbisReader(_stream);

            _samplesPerSecond = AudioSettings.SampleRate * AudioSettings.Channels;

            SendBuffer = new float[_samplesPerSecond / 5 + AudioSettings.HeadSamples];
            ReadBuffer = new float[_samplesPerSecond / 5 + AudioSettings.HeadSamples];

            var cnt = 0;

            Status = AudioStatus.Playing;

            HookRunner.RunEvent(new AudioPlayerStartedPlayingArgs(this));

            while ((cnt = _reader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
            {
                if (_shouldStop)
                {
                    _reader.SeekTo(_reader.TotalSamples - 1);
                    _shouldStop = false;

                    _reader.Dispose();
                    _reader = null;

                    _stream.Dispose();
                    _stream = null;

                    Status = AudioStatus.Idle;

                    yield break;
                }

                while (!_shouldPlay)
                    yield return Timing.WaitForOneFrame;

                while (StreamBuffer.Count >= ReadBuffer.Length)
                {
                    _isReady = true;
                    yield return Timing.WaitForOneFrame;
                }

                for (int i = 0; i < ReadBuffer.Length; i++)
                    StreamBuffer.Enqueue(ReadBuffer[i]);
            }

            Previous = Current;
            Current = null;

            _reader.Dispose();
            _reader = null;

            _stream.Dispose();
            _stream = null;

            Status = AudioStatus.Idle;

            OnFinished.Call();

            if (Flags.Any(AudioFlags.ShuffleNext))
            {
                var list = Queue.ToList();

                list.Shuffle();

                Queue.EnqueueMany(list);
            }

            if (Flags.Any(AudioFlags.LoopCurrent))
                Next = Previous;
            else if (Flags.Any(AudioFlags.PlayNext) && Queue.TryDequeue(out var audioInfo))
                Next = audioInfo;

            HookRunner.RunEvent(new AudioPlayerStoppedPlayingArgs(this));

            if (Next != null)
            {
                Current = Next;
                Next = null;

                _playback = Timing.RunCoroutine(Playback());
            }
        }
    }
}