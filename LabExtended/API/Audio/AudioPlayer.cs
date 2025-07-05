using LabExtended.Extensions;

using MEC;

using NVorbis;

using UnityEngine;

using Utils.Networking;

using VoiceChat;

using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

using VoiceChat.Networking;

namespace LabExtended.API.Audio;

/// <summary>
/// Manages audio playback.
/// </summary>
public class AudioPlayer : IDisposable
{
    /// <summary>
    /// Gets called when playback starts.
    /// </summary>
    public static event Action<AudioPlayer>? Started;

    /// <summary>
    /// Gets called when playback stops (or finishes).
    /// </summary>
    public static event Action<AudioPlayer>? Stopped;

    /// <summary>
    /// Gets called when an audio player is disposed.
    /// </summary>
    public static event Action<AudioPlayer>? Disposed;

    private Predicate<ExPlayer> cachedFilterPlayers;
    
    /// <summary>
    /// Gets the audio player's parent audio group.
    /// </summary>
    public AudioHandler? Handler { get; internal set; }

    /// <summary>
    /// Gets the audio player's speaker toy ID list.
    /// </summary>
    public List<byte> Sources
    {
        get
        {
            if (Handler?.Sources != null)
                return Handler.Sources;

            field ??= new();
            return field;
        }
    } 
    
    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    
    /// <summary>
    /// Whether or not the audio stream buffer is full enough to transmit audio.
    /// </summary>
    public bool IsReady { get; private set; }
    
    /// <summary>
    /// Whether or not the player is playing any audio.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// Whether or not the playback is paused.
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Whether or not the playback is looping (repeating the same audio clip).
    /// </summary>
    public bool IsLooping { get; set; }
    
    /// <summary>
    /// Whether or not the playback should be stopped.
    /// </summary>
    public bool ShouldStop { get; private set; }

    /// <summary>
    /// Gets the amount of samples per second.
    /// </summary>
    public int SamplesPerSecond { get; private set; }

    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    public float Volume { get; set; } = 100f;

    /// <summary>
    /// Gets the amount of samples allowed to be sent.
    /// </summary>
    public float AllowedSamples { get; private set; }

    /// <summary>
    /// Gets the buffer for opus-encoded audio.
    /// </summary>
    public byte[] EncodedBuffer { get; private set; } = new byte[VoiceChatSettings.MaxEncodedSize];
    
    /// <summary>
    /// Gets the buffer used for reading from raw audio.
    /// </summary>
    public float[]? ReadBuffer { get; private set; }

    /// <summary>
    /// Gets the buffer used for encoding raw audio.
    /// </summary>
    public float[]? WriteBuffer { get; private set; }

    /// <summary>
    /// Gets the buffer used for streaming audio.
    /// </summary>
    public Queue<float> StreamQueue { get; private set; } = new();

    /// <summary>
    /// Gets the active playback buffer.
    /// </summary>
    public PlaybackBuffer PlaybackBuffer { get; private set; } = new();
    
    /// <summary>
    /// Gets the active audio reader.
    /// </summary>
    public VorbisReader? Reader { get; private set; } 
    
    /// <summary>
    /// Gets the active audio stream.
    /// </summary>
    public MemoryStream? Stream { get; private set; }

    /// <summary>
    /// Gets the active opus encoder.
    /// </summary>
    public OpusEncoder Encoder { get; private set; } = new(OpusApplicationType.Voip);
    
    /// <summary>
    /// Gets the currently played audio clip.
    /// </summary>
    public KeyValuePair<string, byte[]>? CurrentClip { get; private set; }
    
    /// <summary>
    /// Gets or sets the next audio clip to play.
    /// </summary>
    public KeyValuePair<string, byte[]>? NextClip { get; set; }
    
    /// <summary>
    /// Gets the audio clip queue.
    /// </summary>
    public Queue<KeyValuePair<string, byte[]>> ClipQueue { get; private set; } = new();
    
    /// <summary>
    /// Gets or sets the predicate used to filter which players receive the audio.
    /// </summary>
    public Func<AudioPlayer, ExPlayer, bool>? ReceiveFilter { get; set; }
    
    /// <summary>
    /// Gets the coroutine handle of the playback coroutine.
    /// </summary>
    public CoroutineHandle PlaybackHandle { get; private set; }

    /// <summary>
    /// Plays a specific audio clip.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="overrideCurrent">Whether or not to stop the currently played clip.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Play(KeyValuePair<string, byte[]> clip, bool overrideCurrent = false)
    {
        if (clip.Value is null)
            throw new ArgumentNullException(nameof(clip));

        if (IsPlaying)
        {
            if (overrideCurrent)
            {
                NextClip = clip;
                ShouldStop = true;
            }
            else
            {
                ClipQueue.Enqueue(clip);
            }
        }
        else
        {
            CurrentClip = clip;
            PlaybackHandle = Timing.RunCoroutine(PlaybackCoroutine());
        }
    }

    /// <summary>
    /// Stops the playback.
    /// </summary>
    /// <param name="clearQueue">Whether or not to clear the queue (a new clip will start playing if the queue isn't empty).</param>
    public void Stop(bool clearQueue = false)
    {
        if (IsPlaying)
        {
            if (clearQueue)
            {
                NextClip = null;
                ClipQueue.Clear();
            }

            ShouldStop = true;
        }
    }
    
    /// <summary>
    /// Updates the audio playback.
    /// </summary>
    public void Update()
    {
        if (IsDisposed || !IsReady || StreamQueue is null || StreamQueue.Count == 0 || ShouldStop || WriteBuffer is null)
            return;

        AllowedSamples += Time.deltaTime * SamplesPerSecond;

        var copyCount = Mathf.Min(Mathf.FloorToInt(AllowedSamples), StreamQueue.Count);

        for (var i = 0; i < copyCount; i++)
        {
            PlaybackBuffer.Write(StreamQueue.Dequeue() * (Volume / 100f));
        }

        AllowedSamples -= copyCount;

        while (PlaybackBuffer.Length >= VoiceChatSettings.PacketSizePerChannel)
        {
            PlaybackBuffer.ReadTo(WriteBuffer, VoiceChatSettings.PacketSizePerChannel);

            var encodedLength = Encoder.Encode(WriteBuffer, EncodedBuffer, VoiceChatSettings.PacketSizePerChannel);

            if (cachedFilterPlayers is null)
                cachedFilterPlayers = FilterPlayers;

            if (Handler != null)
            {
                Handler.Transmit(EncodedBuffer, encodedLength, cachedFilterPlayers);
            }
            else
            {
                var audioMessage = new AudioMessage(0, EncodedBuffer, encodedLength);

                for (var index = 0; index < Sources.Count; index++)
                {
                    audioMessage.ControllerId = Sources[index];
                    audioMessage.SendToAuthenticated();
                }
            }
        }
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (IsDisposed)
            return;
        
        IsDisposed = true;
        
        Disposed?.InvokeSafe(this);

        if (PlaybackHandle.IsRunning)
            Timing.KillCoroutines(PlaybackHandle);
        
        if (Handler is null)
            Sources.Clear();
        
        Handler?.InternalRemovePlayer();
        Handler = null;

        ShouldStop = false;

        IsReady = false;
        IsPlaying = false;
        IsLooping = false;
        IsPaused = false;

        EncodedBuffer = null;
        WriteBuffer = null;
        ReadBuffer = null;

        NextClip = null;
        CurrentClip = null;
        
        ClipQueue?.Clear();
        ClipQueue = null;
        
        PlaybackBuffer?.Dispose();
        PlaybackBuffer = null;
        
        Reader?.Dispose();
        Reader = null;
        
        Stream?.Dispose();
        Stream = null;
        
        StreamQueue?.Clear();
        StreamQueue = null;
        
        Encoder?.Dispose();
        Encoder = null;

        PlaybackHandle = default;
    }

    private IEnumerator<float> PlaybackCoroutine()
    {
        if (!CurrentClip.HasValue)
            throw new Exception("Could not start playback - CurrentClip is null");
        
        ShouldStop = false;
        IsReady = false;

        Stream = new(CurrentClip.Value.Value);
        Reader = new(Stream, true);

        SamplesPerSecond = VoiceChatSettings.SampleRate * VoiceChatSettings.Channels;

        WriteBuffer = new float[SamplesPerSecond / 5 + 1920];
        ReadBuffer = new float[SamplesPerSecond / 5 + 1920];
        
        Started?.InvokeSafe(this);

        IsPlaying = true;

        var samples = 0;

        while ((samples = Reader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0)
        {
            if (ShouldStop)
            {
                Reader.SeekTo(Reader.TotalSamples - 1);
                
                ShouldStop = false;
            }

            while (IsPaused)
            {
                yield return Timing.WaitForOneFrame;
            }

            while (StreamQueue.Count > ReadBuffer.Length)
            {
                IsReady = true;
                
                yield return Timing.WaitForOneFrame;
            }

            for (var i = 0; i < ReadBuffer.Length; i++)
            {
                StreamQueue.Enqueue(ReadBuffer[i]);
            }
        }

        while (StreamQueue.Count > 0 && !ShouldStop)
        {
            IsReady = true;

            yield return Timing.WaitForOneFrame;
        }

        IsPlaying = false;
        IsReady = false;
        
        ShouldStop = false;

        if (!NextClip.HasValue)
        {
            if (IsLooping)
            {
                NextClip = CurrentClip;
            }
            else if (ClipQueue.TryDequeue(out var clip))
            {
                NextClip = clip;
            }
        }

        CurrentClip = null;
        
        Reader.Dispose();
        Reader = null;
        
        Stream.Dispose();
        Stream = null;
        
        StreamQueue.Clear();

        WriteBuffer = null;
        ReadBuffer = null;
        
        Stopped?.InvokeSafe(this);

        if (NextClip.HasValue)
        {
            CurrentClip = NextClip;
            NextClip = null;
            PlaybackHandle = Timing.RunCoroutine(PlaybackCoroutine());
        }
    }

    private bool FilterPlayers(ExPlayer player)
    {
        if (ReceiveFilter != null)
            return ReceiveFilter(this, player);

        return true;
    }
}