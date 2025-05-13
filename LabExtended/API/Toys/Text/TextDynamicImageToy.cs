using System.Diagnostics;

using LabExtended.Extensions;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Toys.Text;

/// <summary>
/// A toy that can be used to spawn dynamic images.
/// </summary>
public class TextDynamicImageToy : IDisposable
{
    private List<List<string>>? imageList;
    private List<string>? curList;
    
    private int prevFrameIndex = -1;
    private float targetFrameDelay = 1000f / 24;
    
    /// <summary>
    /// Gets the base text toy.
    /// </summary>
    public TextToy Toy { get; private set; }
    
    /// <summary>
    /// Gets the stopwatch used to measure delay between frames.
    /// </summary>
    public Stopwatch? Watch { get; private set; }

    /// <summary>
    /// Gets or sets the display size.
    /// </summary>
    public Vector2 Size
    {
        get => Toy.Size;
        set => Toy.Size = value;
    }
    
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
    public int FrameCount => imageList.Count;
    
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
    /// Gets the total image duration.
    /// </summary>
    public TimeSpan Duration => imageList != null ? TimeSpan.FromSeconds(FrameCount / FramesPerSecond) : TimeSpan.Zero;
    
    /// <summary>
    /// Gets the remaining image duration.
    /// </summary>
    public TimeSpan Remaining => imageList != null && IsPlaying ? TimeSpan.FromSeconds((FrameCount - (FrameIndex + 1)) / FramesPerSecond) : TimeSpan.Zero;

    /// <summary>
    /// Gets called when a new frame is displayed.
    /// </summary>
    public event Action? Displayed;

    /// <summary>
    /// Gets called when all frames are played.
    /// </summary>
    public event Action? Finished;
    
    /// <summary>
    /// Creates a new <see cref="TextImageToy"/> wrapper instance.
    /// </summary>
    /// <param name="baseToy"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TextDynamicImageToy(TextToy baseToy)
    {
        if (baseToy is null || baseToy.Base == null)
            throw new ArgumentNullException(nameof(baseToy));

        imageList = ListPool<List<string>>.Shared.Rent();
        
        Toy = baseToy;
        Toy.Clear(true);

        Watch = new();

        PlayerUpdateHelper.OnUpdate += OnUpdate;
    }
    
    /// <summary>
    /// Plays the specified list of frames.
    /// </summary>
    /// <param name="frames">The list of frames to play.</param>
    /// <param name="frameRate">The target frame rate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Play(List<List<string>> frames, int? frameRate = null)
    {
        if (frames is null)
            throw new ArgumentNullException(nameof(frames));
        
        if (frames.Count == 0)
            throw new ArgumentException("Frames cannot be empty", nameof(frames));
        
        Clear();
        
        FramesPerSecond = frameRate is > 0 ? frameRate.Value : 24;
        
        imageList.Clear();
        imageList.AddRange(frames);
        
        Watch.Reset();
        Watch.Start();
    }

    /// <summary>
    /// Clears the image.
    /// </summary>
    public void Clear()
    {
        Toy.Clear(true);
        
        imageList.Clear();
        
        if (Watch.IsRunning)
        {
            Watch.Stop();
            Watch.Reset();
        }
        
        FrameIndex = 0;
        prevFrameIndex = -1;
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        PlayerUpdateHelper.OnUpdate -= OnUpdate;
        
        if (imageList != null)
            ListPool<List<string>>.Shared.Return(imageList);

        imageList = null;
        Toy = null;
    }
    
    private void OnUpdate()
    {
        if (!IsPlaying)
            return;

        if (Watch.ElapsedMilliseconds < targetFrameDelay)
            return;

        Watch.Restart();

        if (FrameIndex >= imageList.Count)
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
        curList = imageList[IsPaused ? FrameIndex : FrameIndex++];
        
        Toy.Arguments.Clear();
        Toy.Arguments.AddRange(curList);
        
        Displayed?.InvokeSafe();
    }

    private void OnFinished()
    {
        Finished?.InvokeSafe();
        
        Clear();
    }
}