using System.Diagnostics;
using LabExtended.Extensions;
using LabExtended.Utilities.Update;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Toys.Primitives;

/// <summary>
/// A primitive object used to display image frames with a variable refresh rate.
/// </summary>
public class PrimitiveDynamicImage : IDisposable
{
    private int prevFrameIndex = -1;
    private float targetFrameDelay = 1000f / 24;
    
    /// <summary>
    /// Gets the current list of frames.
    /// </summary>
    public List<UnityEngine.Color?[,]>? Frames { get; private set; }
    
    /// <summary>
    /// Gets the parent primitive image.
    /// </summary>
    public PrimitiveImageToy? Toy { get; private set; }
    
    /// <summary>
    /// Gets the stopwatch used to measure delay between frames.
    /// </summary>
    public Stopwatch? Watch { get; private set; }

    /// <summary>
    /// Gets or sets the amount of frames per second.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int FramesPerSecond
    {
        get => field;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
                                
            field = value;
            targetFrameDelay = 1000f / value;
        }
    }
    
    /// <summary>
    /// Gets the index of the current frame.
    /// </summary>
    public int FrameIndex { get; private set; }
    
    /// <summary>
    /// Gets the amount of frames.
    /// </summary>
    public int FrameCount => Frames.Count;
    
    /// <summary>
    /// Whether an image is being displayed or not.
    /// </summary>
    public bool IsPlaying => Watch.IsRunning;
    
    /// <summary>
    /// Whether the playback is paused or not.
    /// </summary>
    public bool IsPaused { get; set; }
    
    /// <summary>
    /// Whether the playback is looping or not.
    /// </summary>
    public bool IsLooping { get; set; }
    
    /// <summary>
    /// Whether the image should be cleared to white when finished.
    /// </summary>
    public bool ClearOnFinish { get; set; }
    
    /// <summary>
    /// Whether the image should be destroyed when finished.
    /// </summary>
    public bool DestroyOnFinish { get; set; }
    
    /// <summary>
    /// Gets the total image duration.
    /// </summary>
    public TimeSpan Duration => Frames != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    
    /// <summary>
    /// Gets the remaining image duration.
    /// </summary>
    public TimeSpan Remaining => Frames != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;

    /// <summary>
    /// Gets called when a new frame is displayed.
    /// </summary>
    public event Action? Displayed;

    /// <summary>
    /// Gets called when all frames are played.
    /// </summary>
    public event Action? Finished;

    /// <summary>
    /// Creates a new <see cref="PrimitiveDynamicImage"/> instance.
    /// </summary>
    /// <param name="toy">The parent image toy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PrimitiveDynamicImage(PrimitiveImageToy toy)
    {
        Toy = toy ?? throw new ArgumentNullException(nameof(toy));
        Watch = new Stopwatch();

        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }
    
    /// <summary>
    /// Plays the specified list of frames.
    /// </summary>
    /// <param name="frames">The list of frames to play.</param>
    /// <param name="frameRate">The target frame rate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Play(List<UnityEngine.Color?[,]> frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Count == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        Reset();
        
        FramesPerSecond = frameRate is > 0 ? frameRate.Value : 24;
        Frames = frames;
        
        Watch.Reset();
        Watch.Start();
    }

    /// <summary>
    /// Resets the current frame index, time and frame list.
    /// </summary>
    public void Reset()
    {
        if (Watch.IsRunning)
            Watch.Reset();
        
        Frames?.Clear();
        Frames = null;

        FrameIndex = 0;
        prevFrameIndex = -1;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Reset();
        
        PlayerUpdateHelper.OnUpdate -= OnUpdate;
        
        if (Toy != null)
        {
            Toy.Dispose();
            Toy = null;
        }
    }

    private void OnUpdate()
    {
        if (!IsPlaying)
            return;

        if (Watch.ElapsedMilliseconds < targetFrameDelay)
            return;

        Watch.Restart();

        if (FrameIndex >= Frames.Count)
        {
            if (IsLooping)
            {
                FrameIndex = 0;
            }
            else
            {
                OnFinished();
                return;
            }
        }

        if (prevFrameIndex == FrameIndex)
            return;
        
        prevFrameIndex = FrameIndex;
        
        Toy.SetFrame(Frames[IsPaused ? FrameIndex : FrameIndex++]);
        
        Displayed?.InvokeSafe();
    }

    private void OnFinished()
    {
        Finished?.InvokeSafe();
        
        Reset();

        if (DestroyOnFinish)
        {
            Dispose();
            return;
        }

        if (ClearOnFinish && Toy != null)
            Toy.Clear();
    }
    
    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"PrimitiveDynamicImage (Height={Toy?.Height ?? -1}; Width={Toy?.Width ?? -1}; Frames={Frames?.Count ?? -1}; TargetDelay={targetFrameDelay}; CurrentDelay={(Watch.IsRunning ? Watch.ElapsedMilliseconds : -1)}; Fps={FramesPerSecond}; CurrentIndex={FrameIndex}; Duration={Duration}; Remaining={Remaining}; IsPaused={IsPaused})";
}